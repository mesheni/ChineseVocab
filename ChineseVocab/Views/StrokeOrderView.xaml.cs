using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using Microsoft.Maui.Controls;
using ChineseVocab.ViewModels;
using ChineseVocab.Services;

namespace ChineseVocab.Views
{
    public partial class StrokeOrderView : ContentView
    {
        private StrokeOrderViewModel _viewModel;
        private SKPaint _strokePaint;
        private SKPaint _completedStrokePaint;
        private SKPaint _currentStrokePaint;
        private SKPaint _gridPaint;
        private List<SKPoint> _currentDrawingPoints = new List<SKPoint>();
        private bool _isDrawing = false;
        private float _canvasWidth = 0;
        private float _canvasHeight = 0;
        private float _scaleFactor = 1.0f;
        private SKPoint _offset = new SKPoint(0, 0);

        public StrokeOrderView()
        {
            InitializeComponent();
            InitializePaints();
            BindingContextChanged += OnBindingContextChanged;
        }

        private void InitializePaints()
        {
            _strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#3B82F6"),
                StrokeWidth = 6,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            _completedStrokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#10B981"),
                StrokeWidth = 6,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            _currentStrokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#EF4444"),
                StrokeWidth = 8,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            _gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#E5E7EB").WithAlpha(128),
                StrokeWidth = 1,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            };
        }

        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.AnimationUpdated -= OnAnimationUpdated;
            }

            _viewModel = BindingContext as StrokeOrderViewModel;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                _viewModel.AnimationUpdated += OnAnimationUpdated;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StrokeOrderViewModel.CurrentStrokeNumber) ||
                e.PropertyName == nameof(StrokeOrderViewModel.IsAnimating) ||
                e.PropertyName == nameof(StrokeOrderViewModel.ShowGrid) ||
                e.PropertyName == nameof(StrokeOrderViewModel.IsPracticeMode) ||
                e.PropertyName == nameof(StrokeOrderViewModel.DisplayMode))
            {
                DrawingCanvas.InvalidateSurface();
            }
        }

        private void OnAnimationUpdated(object? sender, EventArgs e)
        {
            DrawingCanvas.InvalidateSurface();
        }

        private void OnCanvasPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            var info = e.Info;

            _canvasWidth = info.Width;
            _canvasHeight = info.Height;

            canvas.Clear(SKColors.White);

            if (_viewModel == null)
                return;

            CalculateScaleAndOffset();
            canvas.Save();
            canvas.Translate(_offset.X, _offset.Y);
            canvas.Scale(_scaleFactor, _scaleFactor);

            if (_viewModel.ShowGrid)
                DrawGrid(canvas);

            DrawCompletedStrokes(canvas);
            DrawCurrentStroke(canvas);

            if (_viewModel.IsPracticeMode && _currentDrawingPoints.Count > 1)
                DrawUserStroke(canvas);

            canvas.Restore();
        }

        private void CalculateScaleAndOffset()
        {
            if (_viewModel == null || _canvasWidth == 0 || _canvasHeight == 0)
                return;

            var targetSize = _viewModel.CanvasSize;
            float scaleX = (_canvasWidth - 40) / (float)targetSize.Width;
            float scaleY = (_canvasHeight - 40) / (float)targetSize.Height;
            _scaleFactor = Math.Min(scaleX, scaleY);

            _offset.X = (_canvasWidth - (float)targetSize.Width * _scaleFactor) / 2;
            _offset.Y = (_canvasHeight - (float)targetSize.Height * _scaleFactor) / 2;
        }

        private void DrawGrid(SKCanvas canvas)
        {
            if (_viewModel == null)
                return;

            var size = _viewModel.CanvasSize;
            float width = (float)size.Width;
            float height = (float)size.Height;

            for (float x = 0; x <= width; x += width / 4)
                canvas.DrawLine(x, 0, x, height, _gridPaint);

            for (float y = 0; y <= height; y += height / 4)
                canvas.DrawLine(0, y, width, y, _gridPaint);
        }

        private void DrawCompletedStrokes(SKCanvas canvas)
        {
            if (_viewModel?.Strokes == null || _viewModel.CurrentStrokeNumber <= 1)
                return;

            for (int i = 0; i < _viewModel.CurrentStrokeNumber - 1; i++)
            {
                if (i < _viewModel.Strokes.Count)
                    DrawStroke(canvas, _viewModel.Strokes[i], _completedStrokePaint);
            }
        }

        private void DrawCurrentStroke(SKCanvas canvas)
        {
            if (_viewModel?.CurrentStroke == null)
                return;

            if (_viewModel.IsAnimating && _viewModel.AnimationProgress > 0)
                DrawAnimatedStroke(canvas, _viewModel.CurrentStroke, _viewModel.AnimationProgress);
            else if (!_viewModel.IsPracticeMode)
                DrawStroke(canvas, _viewModel.CurrentStroke, _currentStrokePaint);
        }

        private void DrawAnimatedStroke(SKCanvas canvas, Stroke stroke, double progress)
        {
            if (stroke.Points == null || stroke.Points.Count < 2)
                return;

            var points = stroke.Points;
            float totalLength = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = ConvertToSKPoint(points[i]);
                var p2 = ConvertToSKPoint(points[i + 1]);
                totalLength += SKPoint.Distance(p1, p2);
            }

            float animatedLength = (float)(totalLength * progress);
            float currentLength = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = ConvertToSKPoint(points[i]);
                var p2 = ConvertToSKPoint(points[i + 1]);
                float segmentLength = SKPoint.Distance(p1, p2);

                if (currentLength + segmentLength >= animatedLength)
                {
                    float remainingLength = animatedLength - currentLength;
                    float ratio = remainingLength / segmentLength;
                    var pEnd = new SKPoint(
                        p1.X + (p2.X - p1.X) * ratio,
                        p1.Y + (p2.Y - p1.Y) * ratio
                    );
                    canvas.DrawLine(p1, pEnd, _currentStrokePaint);
                    break;
                }
                else
                {
                    canvas.DrawLine(p1, p2, _currentStrokePaint);
                    currentLength += segmentLength;
                }
            }
        }

        private void DrawStroke(SKCanvas canvas, Stroke stroke, SKPaint paint)
        {
            if (stroke.Points == null || stroke.Points.Count < 2)
                return;

            for (int i = 0; i < stroke.Points.Count - 1; i++)
            {
                var p1 = ConvertToSKPoint(stroke.Points[i]);
                var p2 = ConvertToSKPoint(stroke.Points[i + 1]);
                canvas.DrawLine(p1, p2, paint);
            }
        }

        private void DrawUserStroke(SKCanvas canvas)
        {
            for (int i = 0; i < _currentDrawingPoints.Count - 1; i++)
                canvas.DrawLine(_currentDrawingPoints[i], _currentDrawingPoints[i + 1], _strokePaint);
        }

        private SKPoint ConvertToSKPoint(Services.Point point)
        {
            return new SKPoint((float)point.X, (float)point.Y);
        }

        private void OnCanvasTouch(object? sender, SKTouchEventArgs e)
        {
            if (_viewModel == null || !_viewModel.IsPracticeMode)
                return;

            var canvasPoint = ConvertToCanvasCoordinates(e.Location);

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    _isDrawing = true;
                    _currentDrawingPoints.Clear();
                    _currentDrawingPoints.Add(canvasPoint);
                    e.Handled = true;
                    break;

                case SKTouchAction.Moved:
                    if (_isDrawing)
                    {
                        _currentDrawingPoints.Add(canvasPoint);
                        DrawingCanvas.InvalidateSurface();
                        e.Handled = true;
                    }
                    break;

                case SKTouchAction.Released:
                    if (_isDrawing)
                    {
                        _currentDrawingPoints.Add(canvasPoint);
                        _isDrawing = false;
                        _viewModel.ValidateUserStroke(_currentDrawingPoints);
                        DrawingCanvas.InvalidateSurface();
                        e.Handled = true;
                    }
                    break;

                case SKTouchAction.Cancelled:
                    _isDrawing = false;
                    _currentDrawingPoints.Clear();
                    DrawingCanvas.InvalidateSurface();
                    e.Handled = true;
                    break;
            }
        }

        private SKPoint ConvertToCanvasCoordinates(SKPoint touchPoint)
        {
            float x = (touchPoint.X - _offset.X) / _scaleFactor;
            float y = (touchPoint.Y - _offset.Y) / _scaleFactor;

            if (_viewModel != null)
            {
                var canvasSize = _viewModel.CanvasSize;
                x = Math.Max(0, Math.Min(x, (float)canvasSize.Width));
                y = Math.Max(0, Math.Min(y, (float)canvasSize.Height));
            }

            return new SKPoint(x, y);
        }

        public void RefreshCanvas()
        {
            DrawingCanvas.InvalidateSurface();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent == null)
                DisposePaints();
        }

        private void DisposePaints()
        {
            _strokePaint?.Dispose();
            _completedStrokePaint?.Dispose();
            _currentStrokePaint?.Dispose();
            _gridPaint?.Dispose();
        }
    }
}
