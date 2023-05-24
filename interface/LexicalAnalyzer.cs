using System;
using System.Collections.Generic;

public static class LexicalAnalyzer
{
    // Перечисление для описания типов лексем
    enum TokenType
    {
        Ключевое_слово,    // Ключевое слово
        Идентификатор, // Идентификатор
        Пробел,      // Разделитель
        Оператор_присваивания, // Оператор присваивания
        Разделитель,  // Конец оператора
        Конец_строки,    // Конец строки
        Целое_число,    // Целое число
        Вещественное_число,       // Вещественное число
        Комментарий,   // Комментарии
        Круглые_скобки,    // Круглые скобки
        Неизвестный_токен     // Недопустимый символ
    }

    // Словарь для хранения ключевых слов
    private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
    {
        { "var", TokenType.Ключевое_слово },
        { "int", TokenType.Ключевое_слово },
        { "real", TokenType.Ключевое_слово },
        { "begin", TokenType.Ключевое_слово },
        { "end", TokenType.Ключевое_слово }
    };

    // Метод для проверки, является ли символ буквой
    private static bool IsLetter(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    // Метод для проверки, является ли символ цифрой
    private static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    // Метод для выделения лексем
    private static List<Tuple<int, TokenType, string>> Tokenize(string input)
    {
        List<Tuple<int, TokenType, string>> tokens = new List<Tuple<int, TokenType, string>>(); // Список для хранения лексем
        int i = 0; // Индекс текущего символа
        int line = 1; //Номер текущей строки

        while (i < input.Length)
        {
            char c = input[i];

            // Если текущий символ является разделителем, пропускаем его
            if (char.IsWhiteSpace(c))
            {
                // Если это пробел или перевод строки, добавляем соответствующую лексему
                if (c == ' ')
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Пробел, "(space)"));
                }
                else if (c == '\n')
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Конец_строки, "\\n"));
                    line++;
                }
                else if (c == '\t')
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Пробел, "\\t"));
                }
                i++;
                continue;
            }

            // Если текущий символ является оператором присваивания (=)
            if (c == '=')
            {
                tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Оператор_присваивания, "="));
                i++;
                continue;
            }
            // Если текущий символ является концом оператора (;)
            if (c == ';')
            {
                tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Разделитель, ";"));
                i++;
                continue;
            }

            // Если текущий символ является концом строки (\n)
            if (c == '\n')
            {
                i++;
                continue;
            }

            // Если текущий символ является скобкой [(] - открытие скобки или комментария
            if (c == '(')
            {
                char nextC = input[++i];
                if (nextC == '*')
                {
                    string com = Convert.ToString(c) + nextC;
                    i++;
                    while (i < input.Length && input[i] != '*') //&& input[] != ')')
                    {
                        com += input[i];
                        i++;
                    }
                    com += input[i++];
                    if (input[i++] == ')' )
                    {
                        com += ")";
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Комментарий, com));
                    }
                    else
                    {
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Неизвестный_токен, com));
                    }
                }
                else
                {
                    string str = Convert.ToString(c) + nextC;
                    i++;
                    // Пока не найдем закрывающую скобку 
                    while (i < input.Length && input[i] != ')')
                    {
                        str += input[i];
                        i++;
                    }

                    // Если нашли закрывающую скобку, то добавляем лексему в список
                    if (i < input.Length && input[i] == ')')
                    {
                        str += ")";
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Круглые_скобки, str));
                        i++;
                    }

                    else // Иначе скобка не была закрыта - это ошибка
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Неизвестный_токен, str));
                }
                continue;
            }

            // Если текущий символ является цифрой, то это может быть число
            if (IsDigit(c))
            {
                string number = "";

                // Пока текущий символ является цифрой или точкой, добавляем его к числу
                while (i < input.Length && (IsDigit(input[i]) || input[i] == '.'))
                {
                    number += input[i];
                    i++;
                }
                // Если число закончилось на точку, то это ошибка
                if (number.EndsWith("."))
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Неизвестный_токен, number));
                }
                else // Иначе добавляем лексему в список
                {
                    if (number.Contains(".")) // Если число содержит точку, то это вещественное число
                    {
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Вещественное_число, number));
                    }
                    else // Иначе это целое число
                    {
                        tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Целое_число, number));
                    }
                }

                continue;
            }

            // Если текущий символ является буквой, то это может быть ключевое слово или идентификатор
            if (IsLetter(c))
            {
                string word = "";

                // Пока текущий символ является буквой или цифрой, добавляем его к слову
                while (i < input.Length && (IsLetter(input[i]) || IsDigit(input[i])))
                {
                    word += input[i];
                    i++;
                }

                // Если слово является ключевым, то добавляем лексему в список
                if (keywords.ContainsKey(word))
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, keywords[word], word));
                }
                else // Иначе это идентификатор
                {
                    tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Идентификатор, word));
                }

                continue;
            }

            // Недопустимый символ - добавляем лексему в список
            tokens.Add(new Tuple<int, TokenType, string>(line, TokenType.Неизвестный_токен, c.ToString()));
            i++;
        }

        return tokens;
    }
    //Метод для запуска сканера
    public static string RunScanner(string input)
    {
        var tokens = LexicalAnalyzer.Tokenize(input);
        string result = "";
        foreach (var token in tokens)
        {
            result += $"{token.Item1,-20} {token.Item2,-20} {token.Item3}\n";
        }
        return result;
    }
}

