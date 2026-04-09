using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChineseVocab.Models;
using SQLite;

namespace ChineseVocab.Services
{
    /// <summary>
    /// Сервис для начальной загрузки данных в базу данных.
    /// Загружает базовые типы иероглифов, колоды, карточки HSK1 и другую начальную информацию.
    /// </summary>
    public class DataSeeder
    {
        private readonly IDatabaseService _databaseService;
        private readonly SQLiteAsyncConnection _database;
        private bool _isSeeded = false;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса загрузки данных.
        /// </summary>
        /// <param name="databaseService">Сервис базы данных.</param>
        public DataSeeder(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            // Получаем путь к базе данных для прямого доступа SQLite
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chinesevocab.db3");
            _database = new SQLiteAsyncConnection(databasePath);
        }

        /// <summary>
        /// Выполняет начальную загрузку данных, если она еще не была выполнена.
        /// </summary>
        /// <returns>True, если данные были загружены; false, если данные уже были загружены ранее.</returns>
        public async Task<bool> SeedAsync()
        {
            if (_isSeeded)
            {
                Console.WriteLine("Данные уже были загружены ранее.");
                return false;
            }

            try
            {
                Console.WriteLine("Начало загрузки начальных данных...");

                // Проверяем, есть ли уже данные в базе
                var cardCount = await _databaseService.GetCardCountAsync();
                if (cardCount > 0)
                {
                    Console.WriteLine($"В базе уже есть {cardCount} карточек. Пропускаем загрузку начальных данных.");
                    _isSeeded = true;
                    return false;
                }

                // Загружаем данные в правильном порядке
                await SeedCharacterTypesAsync();
                await SeedSystemDecksAsync();
                await SeedHsk1CardsAsync();
                await SeedExampleSentencesAsync();

                _isSeeded = true;
                Console.WriteLine("Начальные данные успешно загружены!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки начальных данных: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Загружает предопределенные типы иероглифов.
        /// </summary>
        private async Task SeedCharacterTypesAsync()
        {
            try
            {
                var defaultTypes = CharacterType.GetDefaultCharacterTypes();

                foreach (var characterType in defaultTypes)
                {
                    characterType.CreatedDate = DateTime.UtcNow;
                    characterType.ModifiedDate = DateTime.UtcNow;
                    characterType.IsSystem = true; // Системные типы нельзя удалять

                    await _database.InsertAsync(characterType);
                }

                Console.WriteLine($"Загружено {defaultTypes.Count} типов иероглифов.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки типов иероглифов: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создает системные колоды.
        /// </summary>
        private async Task SeedSystemDecksAsync()
        {
            var systemDecks = new List<Deck>
            {
                new Deck("HSK 1", "Базовые слова и иероглифы HSK 1", 1)
                {
                    Color = "#4CAF50",
                    Icon = "🟢",
                    IsSystem = true,
                    SortOrder = 1
                },
                new Deck("HSK 2", "Слова и иероглифы HSK 2", 2)
                {
                    Color = "#2196F3",
                    Icon = "🔵",
                    IsSystem = true,
                    SortOrder = 2
                },
                new Deck("HSK 3", "Слова и иероглифы HSK 3", 3)
                {
                    Color = "#FF9800",
                    Icon = "🟠",
                    IsSystem = true,
                    SortOrder = 3
                },
                new Deck("Избранное", "Ваши избранные карточки", 0)
                {
                    Color = "#F44336",
                    Icon = "❤️",
                    IsSystem = true,
                    IsFavorite = true,
                    SortOrder = 100
                },
                new Deck("На повторение", "Карточки, требующие повторения", 0)
                {
                    Color = "#9C27B0",
                    Icon = "🔄",
                    IsSystem = true,
                    SortOrder = 101
                }
            };

            try
            {
                foreach (var deck in systemDecks)
                {
                    deck.CreatedDate = DateTime.UtcNow;
                    deck.ModifiedDate = DateTime.UtcNow;
                    await _databaseService.CreateDeckAsync(deck);
                }

                Console.WriteLine($"Создано {systemDecks.Count} системных колод.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания системных колод: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Загружает базовые карточки HSK 1.
        /// </summary>
        private async Task SeedHsk1CardsAsync()
        {
            var hsk1Cards = GetHsk1Cards();
            int addedCount = 0;

            try
            {
                // Получаем колоду HSK 1
                var allDecks = await _databaseService.GetAllDecksAsync();
                var hsk1Deck = allDecks.FirstOrDefault(d => d.Name == "HSK 1" && d.IsSystem);

                if (hsk1Deck == null)
                {
                    throw new InvalidOperationException("Колода HSK 1 не найдена.");
                }

                foreach (var card in hsk1Cards)
                {
                    // Создаем карточку
                    card.CreatedDate = DateTime.UtcNow;
                    card.ModifiedDate = DateTime.UtcNow;
                    card.IsActive = true;

                    var cardId = await _databaseService.CreateCardAsync(card);
                    addedCount++;

                    // Добавляем карточку в колоду HSK 1
                    await _databaseService.AddCardToDeckAsync(cardId, hsk1Deck.Id);

                    // Создаем начальную статистику SRS для карточки
                    var srsStat = new SRSStat(cardId);
                    await _databaseService.SaveSRSStatisticsAsync(srsStat);
                }

                // Обновляем счетчик карточек в колоде
                hsk1Deck.CardCount = addedCount;
                await _databaseService.UpdateDeckAsync(hsk1Deck);

                Console.WriteLine($"Загружено {addedCount} карточек HSK 1 в колоду '{hsk1Deck.Name}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки карточек HSK 1: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Загружает примеры предложений для карточек.
        /// </summary>
        private async Task SeedExampleSentencesAsync()
        {
            // Получаем все карточки
            var allCards = await _databaseService.GetAllCardsAsync();
            if (allCards.Count == 0)
            {
                Console.WriteLine("Нет карточек для добавления примеров предложений.");
                return;
            }

            var exampleSentences = GetExampleSentences(allCards);
            int addedCount = 0;

            try
            {
                foreach (var sentence in exampleSentences)
                {
                    sentence.CreatedDate = DateTime.UtcNow;
                    sentence.ModifiedDate = DateTime.UtcNow;
                    sentence.IsActive = true;

                    await _databaseService.CreateSentenceAsync(sentence);
                    addedCount++;
                }

                Console.WriteLine($"Добавлено {addedCount} примеров предложений.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки примеров предложений: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Возвращает список базовых карточек HSK 1.
        /// </summary>
        private List<Card> GetHsk1Cards()
        {
            // Базовые карточки HSK 1 (первые 50 для примера)
            return new List<Card>
            {
                new Card("一", "yī", "один, 1", 1) { StrokeCount = 1, Radical = "一", CharacterType = "指事字", FrequencyRank = 1 },
                new Card("二", "èr", "два, 2", 1) { StrokeCount = 2, Radical = "二", CharacterType = "指事字", FrequencyRank = 2 },
                new Card("三", "sān", "три, 3", 1) { StrokeCount = 3, Radical = "三", CharacterType = "指事字", FrequencyRank = 3 },
                new Card("四", "sì", "четыре, 4", 1) { StrokeCount = 5, Radical = "囗", CharacterType = "象形字", FrequencyRank = 4 },
                new Card("五", "wǔ", "пять, 5", 1) { StrokeCount = 4, Radical = "二", CharacterType = "象形字", FrequencyRank = 5 },
                new Card("六", "liù", "шесть, 6", 1) { StrokeCount = 4, Radical = "八", CharacterType = "指事字", FrequencyRank = 6 },
                new Card("七", "qī", "семь, 7", 1) { StrokeCount = 2, Radical = "一", CharacterType = "指事字", FrequencyRank = 7 },
                new Card("八", "bā", "восемь, 8", 1) { StrokeCount = 2, Radical = "八", CharacterType = "指事字", FrequencyRank = 8 },
                new Card("九", "jiǔ", "девять, 9", 1) { StrokeCount = 2, Radical = "乙", CharacterType = "象形字", FrequencyRank = 9 },
                new Card("十", "shí", "десять, 10", 1) { StrokeCount = 2, Radical = "十", CharacterType = "指事字", FrequencyRank = 10 },

                new Card("人", "rén", "человек, люди", 1) { StrokeCount = 2, Radical = "人", CharacterType = "象形字", FrequencyRank = 11 },
                new Card("大", "dà", "большой", 1) { StrokeCount = 3, Radical = "大", CharacterType = "象形字", FrequencyRank = 12 },
                new Card("小", "xiǎo", "маленький", 1) { StrokeCount = 3, Radical = "小", CharacterType = "象形字", FrequencyRank = 13 },
                new Card("日", "rì", "солнце, день", 1) { StrokeCount = 4, Radical = "日", CharacterType = "象形字", FrequencyRank = 14 },
                new Card("月", "yuè", "луна, месяц", 1) { StrokeCount = 4, Radical = "月", CharacterType = "象形字", FrequencyRank = 15 },
                new Card("山", "shān", "гора", 1) { StrokeCount = 3, Radical = "山", CharacterType = "象形字", FrequencyRank = 16 },
                new Card("水", "shuǐ", "вода", 1) { StrokeCount = 4, Radical = "水", CharacterType = "象形字", FrequencyRank = 17 },
                new Card("火", "huǒ", "огонь", 1) { StrokeCount = 4, Radical = "火", CharacterType = "象形字", FrequencyRank = 18 },
                new Card("木", "mù", "дерево", 1) { StrokeCount = 4, Radical = "木", CharacterType = "象形字", FrequencyRank = 19 },
                new Card("土", "tǔ", "земля, почва", 1) { StrokeCount = 3, Radical = "土", CharacterType = "象形字", FrequencyRank = 20 },

                new Card("上", "shàng", "верх, на", 1) { StrokeCount = 3, Radical = "一", CharacterType = "指事字", FrequencyRank = 21 },
                new Card("下", "xià", "низ, под", 1) { StrokeCount = 3, Radical = "一", CharacterType = "指事字", FrequencyRank = 22 },
                new Card("中", "zhōng", "середина, Китай", 1) { StrokeCount = 4, Radical = "丨", CharacterType = "指事字", FrequencyRank = 23 },
                new Card("国", "guó", "страна", 1) { StrokeCount = 8, Radical = "囗", CharacterType = "会意字", FrequencyRank = 24 },
                new Card("个", "gè", "счетное слово", 1) { StrokeCount = 3, Radical = "丨", CharacterType = "象形字", FrequencyRank = 25 },
                new Card("我", "wǒ", "я, меня", 1) { StrokeCount = 7, Radical = "戈", CharacterType = "象形字", FrequencyRank = 26 },
                new Card("你", "nǐ", "ты, тебя", 1) { StrokeCount = 7, Radical = "人", CharacterType = "形声字", FrequencyRank = 27 },
                new Card("他", "tā", "он, его", 1) { StrokeCount = 5, Radical = "人", CharacterType = "形声字", FrequencyRank = 28 },
                new Card("好", "hǎo", "хороший", 1) { StrokeCount = 6, Radical = "女", CharacterType = "会意字", FrequencyRank = 29 },
                new Card("是", "shì", "быть, да", 1) { StrokeCount = 9, Radical = "日", CharacterType = "会意字", FrequencyRank = 30 },

                new Card("不", "bù", "не, нет", 1) { StrokeCount = 4, Radical = "一", CharacterType = "象形字", FrequencyRank = 31 },
                new Card("有", "yǒu", "иметь, есть", 1) { StrokeCount = 6, Radical = "月", CharacterType = "会意字", FrequencyRank = 32 },
                new Card("在", "zài", "находиться, в", 1) { StrokeCount = 6, Radical = "土", CharacterType = "形声字", FrequencyRank = 33 },
                new Card("来", "lái", "приходить", 1) { StrokeCount = 7, Radical = "木", CharacterType = "象形字", FrequencyRank = 34 },
                new Card("去", "qù", "уходить", 1) { StrokeCount = 5, Radical = "厶", CharacterType = "会意字", FrequencyRank = 35 },
                new Card("说", "shuō", "говорить", 1) { StrokeCount = 9, Radical = "言", CharacterType = "形声字", FrequencyRank = 36 },
                new Card("看", "kàn", "смотреть", 1) { StrokeCount = 9, Radical = "目", CharacterType = "会意字", FrequencyRank = 37 },
                new Card("听", "tīng", "слушать", 1) { StrokeCount = 7, Radical = "口", CharacterType = "形声字", FrequencyRank = 38 },
                new Card("学", "xué", "учиться", 1) { StrokeCount = 8, Radical = "子", CharacterType = "会意字", FrequencyRank = 39 },
                new Card("生", "shēng", "жизнь, рождаться", 1) { StrokeCount = 5, Radical = "生", CharacterType = "象形字", FrequencyRank = 40 },

                new Card("的", "de", "притяжательная частица", 1) { StrokeCount = 8, Radical = "白", CharacterType = "形声字", FrequencyRank = 41 },
                new Card("了", "le", "завершенное действие", 1) { StrokeCount = 2, Radical = "乙", CharacterType = "象形字", FrequencyRank = 42 },
                new Card("吗", "ma", "вопросительная частица", 1) { StrokeCount = 6, Radical = "口", CharacterType = "形声字", FrequencyRank = 43 },
                new Card("呢", "ne", "вопросительная частица", 1) { StrokeCount = 8, Radical = "口", CharacterType = "形声字", FrequencyRank = 44 },
                new Card("很", "hěn", "очень", 1) { StrokeCount = 9, Radical = "彳", CharacterType = "形声字", FrequencyRank = 45 },
                new Card("和", "hé", "и, с", 1) { StrokeCount = 8, Radical = "口", CharacterType = "形声字", FrequencyRank = 46 },
                new Card("这", "zhè", "этот", 1) { StrokeCount = 7, Radical = "辶", CharacterType = "形声字", FrequencyRank = 47 },
                new Card("那", "nà", "тот", 1) { StrokeCount = 6, Radical = "阝", CharacterType = "形声字", FrequencyRank = 48 },
                new Card("里", "lǐ", "внутри", 1) { StrokeCount = 7, Radical = "里", CharacterType = "会意字", FrequencyRank = 49 },
                new Card("家", "jiā", "семья, дом", 1) { StrokeCount = 10, Radical = "宀", CharacterType = "会意字", FrequencyRank = 50 }
            };
        }

        /// <summary>
        /// Возвращает примеры предложений для карточек.
        /// </summary>
        private List<Sentence> GetExampleSentences(List<Card> cards)
        {
            var sentences = new List<Sentence>();
            var random = new Random();

            // Простые предложения для демонстрации
            var templateSentences = new List<(string chinese, string pinyin, string translation)>
            {
                ("我爱你", "wǒ ài nǐ", "Я тебя люблю"),
                ("你好吗？", "nǐ hǎo ma?", "Как дела?"),
                ("这是什么？", "zhè shì shénme?", "Что это?"),
                ("我去学校", "wǒ qù xuéxiào", "Я иду в школу"),
                ("他是我朋友", "tā shì wǒ péngyou", "Он мой друг"),
                ("今天天气很好", "jīntiān tiānqì hěn hǎo", "Сегодня погода очень хорошая"),
                ("我会说中文", "wǒ huì shuō zhōngwén", "Я умею говорить по-китайски"),
                ("她很好看", "tā hěn hǎokàn", "Она очень красивая"),
                ("我们学习汉语", "wǒmen xuéxí hànyǔ", "Мы изучаем китайский язык"),
                ("这是中国的茶", "zhè shì zhōngguó de chá", "Это китайский чай")
            };

            try
            {
                // Для каждой карточки создаем 1-2 примера предложений
                foreach (var card in cards.Take(20)) // Ограничим для демонстрации
                {
                    // Создаем простое предложение с использованием иероглифа
                    var sentenceTemplate = templateSentences[random.Next(templateSentences.Count)];
                    var sentence = new Sentence(card.Id, sentenceTemplate.chinese, sentenceTemplate.pinyin, sentenceTemplate.translation)
                    {
                        DifficultyLevel = 1,
                        Source = "Пример",
                        Explanation = $"Иероглиф '{card.Character}' ({card.Pinyin}) означает '{card.Definition}'."
                    };
                    sentences.Add(sentence);

                    // Для некоторых карточек добавляем второй пример
                    if (random.NextDouble() > 0.7)
                    {
                        var anotherTemplate = templateSentences[random.Next(templateSentences.Count)];
                        var anotherSentence = new Sentence(card.Id, anotherTemplate.chinese, anotherTemplate.pinyin, anotherTemplate.translation)
                        {
                            DifficultyLevel = 2,
                            Source = "Пример 2",
                            Explanation = $"Дополнительный пример использования иероглифа '{card.Character}'."
                        };
                        sentences.Add(anotherSentence);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания примеров предложений: {ex.Message}");
            }

            return sentences;
        }

        /// <summary>
        /// Сбрасывает флаг загрузки данных (для тестирования).
        /// </summary>
        public void Reset()
        {
            _isSeeded = false;
        }
    }
}
