using System;
using System.Collections.Generic;
using System.Globalization;

namespace LABA1
{
    public class SpreadsheetEngine
    {
        private Dictionary<string, Cell> _cells = new Dictionary<string, Cell>();

        public void SetCellFormula(string cellName, string formula)
        {
            if (!_cells.ContainsKey(cellName))
            {
                _cells[cellName] = new Cell(cellName);
            }
            _cells[cellName].Formula = formula;
        }

        public string GetCellFormula(string cellName)
        {
            return _cells.ContainsKey(cellName) ? _cells[cellName].Formula : "";
        }

        public string GetCellValue(string cellName)
        {
            if (!_cells.ContainsKey(cellName)) return "";
            return _cells[cellName].Value.ToString(CultureInfo.InvariantCulture);
        }

        public void CalculateAll()
        {
            foreach (var cell in _cells.Values)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(cell.Formula))
                    {
                        cell.Value = 0;
                        continue;
                    }
                    var parser = new ExpressionParser(cell.Formula, _cells);
                    cell.Value = parser.Parse();
                }
                catch
                {
                    cell.Value = double.NaN;
                }
            }
        }

        public static string GetColumnName(int index)
        {
            return ((char)('A' + index)).ToString();
        }
    }

    public class Cell
    {
        public string Name { get; set; }
        public string Formula { get; set; }
        public double Value { get; set; }

        public Cell(string name)
        {
            Name = name;
            Formula = "";
            Value = 0;
        }
    }

    public class ExpressionParser
    {
        private string _expression;
        private int _pos;
        private Dictionary<string, Cell> _context;

        public ExpressionParser(string expression, Dictionary<string, Cell> context)
        {
            _expression = expression;
            _context = context;
            _pos = 0;
        }

        public double Parse()
        {
            var result = ParseOr();
            SkipWhitespace();
            if (_pos < _expression.Length) throw new Exception("Error");
            return result;
        }

        private void SkipWhitespace()
        {
            while (_pos < _expression.Length && char.IsWhiteSpace(_expression[_pos]))
            {
                _pos++;
            }
        }


        private double ParseOr()
        {
            double left = ParseAnd();
            while (Match("or"))
            {
                double right = ParseAnd();
                left = ((left != 0) || (right != 0)) ? 1 : 0;
            }
            return left;
        }

        private double ParseAnd()
        {
            double left = ParseRelational();
            while (Match("and"))
            {
                double right = ParseRelational();
                left = ((left != 0) && (right != 0)) ? 1 : 0;
            }
            return left;
        }

        private double ParseRelational()
        {
            double left = ParseAddSub();
            if (Match("=")) { double right = ParseAddSub(); left = (Math.Abs(left - right) < 0.000001) ? 1 : 0; }
            else if (Match("<")) { double right = ParseAddSub(); left = (left < right) ? 1 : 0; }
            else if (Match(">")) { double right = ParseAddSub(); left = (left > right) ? 1 : 0; }
            return left;
        }

        private double ParseAddSub()
        {
            double left = ParseMulDiv();
            while (true)
            {
                if (Match("+")) left += ParseMulDiv();
                else if (Match("-")) left -= ParseMulDiv();
                else break;
            }
            return left;
        }

        private double ParseMulDiv()
        {
            double left = ParseUnary();
            while (true)
            {
                if (Match("*")) left *= ParseUnary();
                else if (Match("/")) { double right = ParseUnary(); left /= right; }
                else break;
            }
            return left;
        }

        private double ParseUnary()
        {
            if (Match("not")) return (ParseUnary() == 0) ? 1 : 0;
            if (Match("-")) return -ParseUnary();
            return ParseFactor();
        }

        private double ParseFactor()
        {
            SkipWhitespace(); 

            if (_pos >= _expression.Length) throw new Exception("Unexpected end");

            if (char.IsDigit(Peek()) || Peek() == '.') return ParseNumber();

            if (Match("(")) { double res = ParseOr(); Match(")"); return res; }

            if (Match("max")) { Match("("); double v1 = ParseOr(); Match(","); double v2 = ParseOr(); Match(")"); return Math.Max(v1, v2); }
            if (Match("min")) { Match("("); double v1 = ParseOr(); Match(","); double v2 = ParseOr(); Match(")"); return Math.Min(v1, v2); }

            string id = ParseIdentifier();
            if (!string.IsNullOrEmpty(id) && _context.ContainsKey(id.ToUpper()))
                return _context[id.ToUpper()].Value;

            throw new Exception("Unknown");
        }

        private double ParseNumber()
        {
            SkipWhitespace();
            string s = "";
            while (_pos < _expression.Length && (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.'))
            {
                s += _expression[_pos++];
            }
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double res)) return res;
            return 0;
        }

        private string ParseIdentifier()
        {
            SkipWhitespace();
            string s = "";
            while (_pos < _expression.Length && char.IsLetterOrDigit(_expression[_pos]))
            {
                s += _expression[_pos++];
            }
            return s;
        }

        private bool Match(string t)
        {
            SkipWhitespace();

            if (_expression.Length - _pos >= t.Length)
            {
                string sub = _expression.Substring(_pos, t.Length);
                if (sub.Equals(t, StringComparison.OrdinalIgnoreCase))
                {
                    _pos += t.Length;
                    return true;
                }
            }
            return false;
        }

        private char Peek()
        {
            SkipWhitespace();
            return _pos < _expression.Length ? _expression[_pos] : '\0';
        }
    }
}
