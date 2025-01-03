using System;
using System.Diagnostics;
using System.Linq;

namespace HedgehogMeetings
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            bool continueRunning = true;

            while (continueRunning)
            {
                try
                {
                    // Input validation
                    Console.WriteLine("Enter number of hedgehogs of each color (three space-separated numbers) or 'q' - to quit:");

                    string input = Console.ReadLine();
                    if (input.Trim().ToLower() == "q")
                    {
                        continueRunning = false;
                        continue;
                    }

                    if (!TryParseInput(input, out int[] population))
                    {
                        Console.WriteLine("Invalid input! Enter three non-negative integers separated by spaces also ensure that input is between -1 and (2^31 - 1).");
                        continue;
                    }

                    Console.WriteLine("Enter target color (0 - red, 1 - green, 2 - blue):");
                    if (!int.TryParse(Console.ReadLine()?.Trim(), out int targetColor) || targetColor < 0 || targetColor > 2)
                    {
                        Console.WriteLine("Invalid color! Must be a number from 0 to 2.");
                        continue;
                    }

                    // Start measuring time
                    stopwatch.Start();
                    int result = MinimumMeetings(population, targetColor);
                    if (result < -1 || result > int.MaxValue)
                    {
                        Console.WriteLine("Output out of bounds! Ensure result is between -1 and (2^31 - 1).");
                        continue;
                    }

                    Console.WriteLine(result == -1
                        ? "Impossible to achieve desired result."
                        : $"Minimum number of meetings required: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    // Stop measuring time
                    stopwatch.Stop();
                    Console.WriteLine($"\nExecution Time: {stopwatch.ElapsedMilliseconds} ms");
                    // Memory usage output
                    Console.WriteLine($"Memory Usage: {Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0):F2} MiB");

                    Console.WriteLine("\nPress Enter to continue or 'q' to quit...");
                    if (Console.ReadLine()?.Trim().ToLower() == "q")
                    {
                        continueRunning = false;
                    }
                }
            }
        }

        private static bool TryParseInput(string input, out int[] population)
        {
            population = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string[] parts = input.Trim().Split(' ');
            if (parts.Length != 3) return false;

            population = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (!int.TryParse(parts[i], out population[i]) || population[i] < 0 || population[i] > int.MaxValue)
                {
                    return false;
                }
            }

            return true;
        }

        public static int MinimumMeetings(int[] population, int targetColor)
        {
            // Перевірка базових випадків
            long total = population.Sum(p => (long)p);
            if (total == 0) return -1; // Немає їжаків
            if (population[targetColor] == total) return 0; // Усі вже цільового кольору

            int[] colors = population.ToArray();
            int meetings = 0;

            while (true)
            {
                // Перевірка, чи всі їжаки цільового кольору
                if (colors[targetColor] == total)
                    return meetings;

                bool madeProgress = false;

                // 1. Змішуємо нецільові кольори
                for (int i = 0; i < 3 && !madeProgress; i++)
                {
                    if (i == targetColor) continue;
                    for (int j = i + 1; j < 3; j++)
                    {
                        if (j == targetColor) continue;
                        if (colors[i] > 0 && colors[j] > 0)
                        {
                            int pairs = Math.Min(colors[i], colors[j]);
                            colors[i] -= pairs;
                            colors[j] -= pairs;
                            colors[targetColor] += pairs;
                            meetings += pairs;
                            madeProgress = true;
                            break;
                        }
                    }
                }

                if (colors[targetColor] != total)
                    madeProgress = false;

                // 2. Якщо не вдалося змішати нецільові кольори, пробуємо обробити залишки
                if (!madeProgress)
                {
                    // Шукаємо нецільові кольори з найбільшою кількістю
                    int nonTargetSum = 0;
                    int maxNonTarget = -1;

                    for (int i = 0; i < 3; i++)
                    {
                        if (i != targetColor)
                        {
                            nonTargetSum += colors[i];
                            if (maxNonTarget == -1 || colors[i] > colors[maxNonTarget])
                                maxNonTarget = i;
                        }
                    }

                    // Якщо залишки непарні, розв'язок неможливий
                    if (nonTargetSum % 2 != 0)
                        return -1;

                    // Обробка випадку, коли залишок більший за цільовий колір
                    if (colors[maxNonTarget] > colors[targetColor])
                    {
                        while (colors[maxNonTarget] >= 2 && colors[targetColor] >= 1)
                        {
                            // Крок 1: Змішуємо цільовий та нецільовий кольори для отримання третього
                            colors[targetColor] -= 1;
                            colors[maxNonTarget] -= 1;
                            int thirdColor = 3 - maxNonTarget - targetColor;
                            colors[thirdColor] += 1;
                            meetings += 1;

                            // Крок 2: Змішуємо третій колір з нецільовим для повернення цільового
                            if (colors[maxNonTarget] > 0 && colors[thirdColor] > 0)
                            {
                                colors[thirdColor] -= 1;
                                colors[maxNonTarget] -= 1;
                                colors[targetColor] += 1;
                                meetings += 1;
                            }
                            else
                            {
                                break;
                            }

                            madeProgress = true;
                        }
                    }
                    // Обробка випадку, коли залишок менший за цільовий колір
                    if (!madeProgress)
                    {
                        // Змішуємо цільовий колір із найбільшим нецільовим
                        int resultColor = 3 - maxNonTarget - targetColor;
                        int pairs = Math.Min(colors[maxNonTarget], colors[targetColor]);

                        if (pairs >= 0)
                        {
                            colors[maxNonTarget] -= pairs;
                            colors[targetColor] -= pairs;
                            colors[resultColor] += pairs;
                            meetings += pairs;
                            if (colors[targetColor] != total)
                                return meetings;
                        }
                        else
                        {
                           //
                            int partialPairs = colors[maxNonTarget] / 2;
                            if (partialPairs > 0)
                            {
                                colors[maxNonTarget] -= partialPairs * 2;
                                colors[resultColor] += partialPairs;
                                meetings += partialPairs;
                                madeProgress = true;
                            }
                        }
                    }
                }

                // Якщо нічого не вдалося зробити, розв'язок неможливий
                if (!madeProgress)
                    return -1;
            }
        }
    }
}
