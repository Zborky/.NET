using System;

class Program
{
    static void Main()
    {
        int[] arr = { 2, 3, 4, 10, 40 };
        int x = 10;

        int result = BinarySearch(arr, x);
        if (result == -1)
            Console.WriteLine("Element nie je v poli.");
        else
            Console.WriteLine("Element je na indexe: " + result);
    }

    static int BinarySearch(int[] arr, int x)
    {
        int left = 0, right = arr.Length - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            // Skontroluj, či je x na mid
            if (arr[mid] == x)
                return mid;

            // Ak x je väčšie, ignoruj ľavú polovicu
            if (arr[mid] < x)
                left = mid + 1;
            // Ak x je menšie, ignoruj pravú polovicu
            else
                right = mid - 1;
        }

        // Ak sa dospeje sem, znamená to, že element neexistuje
        return -1;
    }
}
