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
            List<string> pairs = PrepareText(text, out Dictionary<int, char> special);

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
            foreach (var item in special)
            {
                result.Insert(item.Key, item.Value);
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

        public List<string> PrepareText(string text, out Dictionary<int, char> special)
        {

            special = new Dictionary<int, char>();

            for (int i = 0; i < text.Length; i++)
            {
                // store special char into dict
                if (!char.IsLetter(text[i]))
                {
                    special.Add(i, text[i]);
                }
            }

            // list of pairs split from text
            List<string> pairs = new List<string>();
            // change text to uppercase and replace J to I
            text = text.ToUpper().Replace("J", "I");
            // eliminate all special char
            text = new string(text.Where(char.IsLetter).ToArray());

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

        // ******* Vigenere repeat key *******
        public string VigenereRepeatEncrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();

            int keyIndex = 0;

            foreach (char c in text)
            {
                // skip the step if it is not a letter
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                // create shift value that repeat from key to move the char of text
                int shift = key[keyIndex % key.Length] - 'A';

                // first subtract the char with A = 26 to have its position then add with shift value to move its position. Finally mod it with 26 to roll back to the start of alphabet if the char after move's value is out of alphabet and add it with A = 26 to have result
                char encrypted = (char)(((c - 'A' + shift) % 26) + 'A');

                result.Append(encrypted);

                // move to another char of key
                keyIndex++;
            }
            return result.ToString();
        }

        public string VigenereAutoEncrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();
            // generate key based on text
            StringBuilder fullkey = new StringBuilder(key);
            // add untill the key has the length equal to the text
            for (int i = 0; fullkey.Length < text.Length && i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    fullkey.Append(text[i]);
                }
            }

            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = fullkey[keyIndex] - 'A';
                char encrypted = (char)(((c - 'A') + shift) % 26 + 'A');

                result.Append(encrypted);

                keyIndex++;
            }
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

            Dictionary<int, char> special = new Dictionary<int, char>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsLetter(text[i]))
                {
                    special.Add(i, text[i]);
                }
            }
            text = new string(text.Where(char.IsLetter).ToArray());
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

            foreach (var item in special)
            {
                result.Insert(item.Key, item.Value);
            }

            return result.ToString();
        }
        // ******* Vigenere *******
        public string VigenereRepeatDecrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();
            
            StringBuilder result = new StringBuilder();

            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = key[keyIndex % key.Length] - 'A';
                char decrypted = (char)(((c - 'A' - shift + 26) % 26) + 'A');
                result.Append(decrypted);
                keyIndex++;
            }
            return result.ToString();
        }

        public string VigenereAutoDecrypt(string text, string key)
        {
            text = text.ToUpper();
            key = key.ToUpper();

            StringBuilder result = new StringBuilder();
            StringBuilder fullkey = new StringBuilder(key);
            int keyIndex = 0;

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result.Append(c);
                    continue;
                }

                int shift = fullkey[keyIndex] - 'A';
                char decrypted = (char)(((c - 'A') + 26 - shift) % 26 + 'A');
                
                result.Append(decrypted);
                fullkey.Append(decrypted);

                keyIndex++;
            }

            return result.ToString();
        }
    }
}