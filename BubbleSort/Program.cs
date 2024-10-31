using System;

class Program
{
    static void Sort(int[] arr) // Zmenené z BubbleSort na Sort
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    // Swap arr[j] a arr[j + 1]
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }
    }

    static void Main()
    {
        int[] arr = { 64, 34, 25, 12, 22, 11, 90 };
        Sort(arr); // Zmenené z BubbleSort na Sort
        Console.WriteLine("Zaradené pole: " + string.Join(", ", arr));
    }
}
