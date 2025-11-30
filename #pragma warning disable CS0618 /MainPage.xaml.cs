#pragma warning disable CS0618 // Ігнорувати застарілі методи
#pragma warning disable CS8618 // Ігнорувати null при ініціалізації
#pragma warning disable CS8602 // Ігнорувати можливий null при зверненні

using Microsoft.Maui.Controls;
using System;

namespace LABA1
{
    public partial class MainPage : ContentPage
    {
        private const int MaxItems = 20;

        private int _rowCount = 5;
        private int _colCount = 5;

        private SpreadsheetEngine _engine;
        private Entry[,] _uiCells;

        public MainPage()
        {
            InitializeComponent();

            _engine = new SpreadsheetEngine();

            RefreshGrid();

            if (ModePicker != null)
                ModePicker.SelectedIndex = 0;
        }

        private void RefreshGrid()
        {
            SpreadsheetGrid.IsVisible = false;

            try
            {
                SpreadsheetGrid.Children.Clear();
                SpreadsheetGrid.RowDefinitions.Clear();
                SpreadsheetGrid.ColumnDefinitions.Clear();

                _uiCells = new Entry[_rowCount + 1, _colCount + 1];

                for (int r = 0; r <= _rowCount; r++)
                    SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                for (int c = 0; c <= _colCount; c++)
                    SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });

                for (int c = 1; c <= _colCount; c++)
                {
                    var label = new Label
                    {
                        Text = SpreadsheetEngine.GetColumnName(c - 1),
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    };
                    SpreadsheetGrid.Add(label, c, 0);
                }

                for (int r = 1; r <= _rowCount; r++)
                {
                    var label = new Label
                    {
                        Text = r.ToString(),
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    };
                    SpreadsheetGrid.Add(label, 0, r);

                    for (int c = 1; c <= _colCount; c++)
                    {
                        var entry = new Entry { BackgroundColor = Colors.White, TextColor = Colors.Black };
                        string cellName = $"{SpreadsheetEngine.GetColumnName(c - 1)}{r}";

                        if (ModePicker != null && ModePicker.SelectedIndex == 1)
                        {
                            _engine.SetCellFormula(cellName, _engine.GetCellFormula(cellName));
                            var val = _engine.GetCellValue(cellName);
                            entry.Text = val == "" ? "0" : val;
                            entry.IsReadOnly = true;

                            if (val == "NaN") entry.TextColor = Colors.Red;
                            else entry.TextColor = Colors.Blue;
                        }
                        else
                        {
                            entry.Text = _engine.GetCellFormula(cellName);
                            entry.IsReadOnly = false;
                        }

                        entry.Unfocused += (s, e) =>
                        {
                            if (ModePicker != null && ModePicker.SelectedIndex == 0)
                            {
                                _engine.SetCellFormula(cellName, entry.Text);
                            }
                        };

                        _uiCells[r, c] = entry;
                        SpreadsheetGrid.Add(entry, c, r);
                    }
                }
            }
            finally
            {
                SpreadsheetGrid.IsVisible = true;
            }
        }

        private async void OnAddRow(object sender, EventArgs e)
        {
            if (_rowCount >= MaxItems) { await DisplayAlert("Ліміт", "Максимум 20", "ОК"); return; }
            _rowCount++;
            RefreshGrid();
        }

        private void OnRemoveRow(object sender, EventArgs e)
        {
            if (_rowCount > 1) { _rowCount--; RefreshGrid(); }
        }

        private async void OnAddCol(object sender, EventArgs e)
        {
            if (_colCount >= MaxItems) { await DisplayAlert("Ліміт", "Максимум 20", "ОК"); return; }
            _colCount++;
            RefreshGrid();
        }

        private void OnRemoveCol(object sender, EventArgs e)
        {
            if (_colCount > 1) { _colCount--; RefreshGrid(); }
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            if (_uiCells == null || _uiCells[1, 1] == null) return;

            SpreadsheetGrid.IsVisible = false;
            try
            {
                if (ModePicker.SelectedIndex == 0) // Формули
                {
                    for (int r = 1; r <= _rowCount; r++)
                    {
                        for (int c = 1; c <= _colCount; c++)
                        {
                            string cellName = $"{SpreadsheetEngine.GetColumnName(c - 1)}{r}";
                            _uiCells[r, c].Text = _engine.GetCellFormula(cellName);
                            _uiCells[r, c].IsReadOnly = false;
                            _uiCells[r, c].TextColor = Colors.Black;
                        }
                    }
                }
                else // Значення
                {
                    _engine.CalculateAll();

                    for (int r = 1; r <= _rowCount; r++)
                    {
                        for (int c = 1; c <= _colCount; c++)
                        {
                            string cellName = $"{SpreadsheetEngine.GetColumnName(c - 1)}{r}";
                            var val = _engine.GetCellValue(cellName);

                            _uiCells[r, c].Text = val;
                            _uiCells[r, c].IsReadOnly = true;

                            if (val == "NaN") _uiCells[r, c].TextColor = Colors.Red;
                            else _uiCells[r, c].TextColor = Colors.Blue;
                        }
                    }
                }
            }
            finally
            {
                SpreadsheetGrid.IsVisible = true;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Інфо", $"Таблицю збережено. Розмір: {_rowCount}x{_colCount}", "ОК");
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лаб. робота. Варіант 54.\nmax, min, not, and, or, <, >, =", "ОК");
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current?.Quit();
        }
    }
}
