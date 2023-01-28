using Iot.Device.CharacterLcd;

namespace PiAccess.Web
{
    public class Lcd2004Writer
    {
        private readonly Lcd2004 _lcd;
        private int _currentLine = 0;
        private int _currentTextIndex = 0;
        private string _currentText = string.Empty;

        public Lcd2004Writer(Lcd2004 lcd)
        {
            _lcd = lcd;
            ClearScreen();
        }

        public void Write(string text)
        {
            _currentText = text;
            var lines = _currentText.Split('\n');
            foreach(var line in lines)
            {
                _lcd.Write(line.Trim());
                _currentLine = (_currentLine == 2) ? 0 : _currentLine + 1;
                _lcd.SetCursorPosition(0, _currentLine);
            }
        }

        public void ShiftCurrentTextLeft(int numOfCharsToShift = 1)
        {
            _currentTextIndex += numOfCharsToShift;
            if (_currentTextIndex >= _currentText.Length)
            {
                _currentTextIndex = 0;
            }
            _lcd.Write(_currentText);
            _currentLine = (_currentLine == 3) ? 0 : _currentLine + 1;
            var textToWrite = _currentText.Substring(_currentTextIndex);
            ClearScreen();
            _lcd.Write(textToWrite);
            _currentLine = (_currentLine == 3) ? 0 : _currentLine + 1;
        }

        public void ClearScreen()
        {
            _lcd.Clear();
            _currentLine = 0;
            _lcd.SetCursorPosition(0, _currentLine);
        }
    }
}
