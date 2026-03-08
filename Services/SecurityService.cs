using System.Text;

namespace ProjectSecurity.Services
{
    public class SecurityService
    {

        // ===== Encrypt =====
        // ******* Ceasar *******
        public string CaesarEncrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c + key));
            }

            return result.ToString();
        }

        // ******* Playfair *******
        public string PlayfairEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            // create key matrix
            char[,] keyMatrix = CreateKeyMatrix(key);
            // split text into list of pair
            List<string> pairs = PrepareText(text);

            foreach (var pair in pairs)
            {
                char a = pair[0];
                char b = pair[1];

                FindPosition(keyMatrix, a, out int r1, out int c1);
                FindPosition(keyMatrix, b, out int r2, out int c2);

                // if 2 char have the same row in the matrix then move the col forward
                if (r1 == r2)
                {
                    result.Append(keyMatrix[r1, (c1 + 1) % 5]);
                    result.Append(keyMatrix[r2, (c2 + 1) % 5]);
                }
                // if 2 char have the same col in the matrix then move the row forward
                else if (c1 == c2)
                {
                    result.Append(keyMatrix[(r1 + 1) % 5, c1]);
                    result.Append(keyMatrix[(r2 + 1) % 5, c2]);
                }
                // if it is random char, create a rectangle and first char is another top corner, second char is another bottom corner
                else
                {
                    result.Append(keyMatrix[r1, c2]);
                    result.Append(keyMatrix[r2, c1]);
                }
            }
            return result.ToString();
        }

        public char[,] CreateKeyMatrix(string key)
        {
            // chang to uppercase and replace J to I
            key = key.ToUpper().Replace("J", "I");
            
            string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

            // use HashSet to ignore duplication char
            HashSet<char> used = new HashSet<char>();
            List<char> result = new List<char>();

            // remove dup char from key and add to result
            foreach (char c in key)
            {
                if (char.IsLetter(c) && !used.Contains(c))
                {
                    used.Add(c);
                    result.Add(c);
                }
            }

            // add remain alphabet char and remove dup char
            foreach (char c in alphabet)
            {
                if (!used.Contains(c))
                {
                    used.Add(c);
                    result.Add(c);
                }
            }

            // create 5x5 martrix
            char[,] matrix = new char[5, 5];
            for (int i = 0; i < 25; i++)
            {
                matrix[i / 5, i % 5] = result[i];
            }

            return matrix;
        }

        public void FindPosition(char[,] matrix, char c, out int row, out int col)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (matrix[i, j] == c)
                    {
                        row = i;
                        col = j;
                        return;
                    }
                }
            }

            row = -1;
            col = -1;
        }

        public List<string> PrepareText(string text)
        {
            // change text to uppercase and replace J to I
            text = text.ToUpper().Replace("J", "I");
            // eliminate all non-text char
            text = new string(text.Where(char.IsLetter).ToArray());
            // list of pairs split from text
            List<string> pairs = new List<string>();

            for (int i = 0; i < text.Length; i+=2)
            {
                // add X if the char is at the end of text
                if (i + 1 >= text.Length)
                {
                    pairs.Add(text[i] + "X");
                }
                // add X between dup char
                else if (text[i] == text[i+1])
                {
                    pairs.Add(text[i] + "X");
                    i--; // back to the second dup char
                }
                // add pair of char and the next char
                else
                {
                    pairs.Add(text[i].ToString() + text[i+1]);
                }
            }

            return pairs;
        }

        // ******* Vigenere *******
        public string VigenereEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            return result.ToString();
        }

        // ===== Decrypt =====
        // ******* Ceasar *******
        public string CaesarDecrypt(string text, int key)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                result.Append((char)(c - key));
            }

            return result.ToString();
        }

        // ******* Playfair *******
        public string PlayfairDecrypt(string text, string key)
        {
            char[,] matrix = CreateKeyMatrix(key);

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < text.Length; i += 2)
            {
                char a = text[i];
                char b = text[i + 1];

                FindPosition(matrix, a, out int r1, out int c1);
                FindPosition(matrix, b, out int r2, out int c2);

                if (r1 == r2)
                {
                    result.Append(matrix[r1, (c1 + 4) % 5]);
                    result.Append(matrix[r2, (c2 + 4) % 5]);
                }
                else if (c1 == c2)
                {
                    result.Append(matrix[(r1 + 4) % 5, c1]);
                    result.Append(matrix[(r2 + 4) % 5, c2]);
                }
                else
                {
                    result.Append(matrix[r1, c2]);
                    result.Append(matrix[r2, c1]);
                }
            }

            return result.ToString();
        }
        // ******* Vigenere *******
        public string VigenereDecrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            return result.ToString();
        }
    }
}