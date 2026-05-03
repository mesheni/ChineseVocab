using SQLite;

namespace ChineseVocab.Models
{
    /// <summary>
    /// Модель связи многие-ко-многим между карточками и колодами.
    /// Соответствует таблице CardDeck в базе данных.
    /// </summary>
    public class CardDeck
    {
        /// <summary>
        /// Идентификатор карточки.
        /// </summary>
        [NotNull]
        public int CardId { get; set; }

        /// <summary>
        /// Идентификатор колоды.
        /// </summary>
        [NotNull]
        public int DeckId { get; set; }

        /// <summary>
        /// Дата добавления карточки в колоду.
        /// </summary>
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public CardDeck() { }

        public CardDeck(int cardId, int deckId)
        {
            CardId = cardId;
            DeckId = deckId;
            AddedDate = DateTime.UtcNow;
        }
    }
}
