using System;
using System.Diagnostics;
using System.Linq;

static long HeavyCalculation(int number)
{
    long result = 0;

    for (int i = 1; i <= 10_000; i++)
    {
        result += ((long)number * i) % 1234567;
    }
    return result;
}

var numbers = Enumerable.Range(1, 5_000_000);
Stopwatch stopwatch = new Stopwatch();
Console.WriteLine("LINQ:");
stopwatch.Start();
var linqResult = numbers
    .Select(HeavyCalculation)
    .ToList();

stopwatch.Stop();
long linqTime = stopwatch.ElapsedMilliseconds;
Console.WriteLine("Time: " + linqTime + " ms");
Console.WriteLine();

Console.WriteLine("PLINQ:");
stopwatch.Restart();

var plinqResult = numbers
    .AsParallel()
    .Select(HeavyCalculation)
    .ToList();

stopwatch.Stop();
long plinqTime = stopwatch.ElapsedMilliseconds;
Console.WriteLine("Time: " + plinqTime + " milliseconds");
Console.WriteLine();

Console.WriteLine("Performance: ");
Console.WriteLine();
Console.WriteLine("LINQ:  " + linqTime + " milliseconds");
Console.WriteLine("PLINQ: " + plinqTime + " milliseconds");
Console.WriteLine();
Console.WriteLine("Speedup: " + Math.Round((double)linqTime / plinqTime, 2) + "x");
Console.WriteLine();

Console.WriteLine("Degree of Parallelism");
Console.WriteLine();

int[] degrees = { 2, 4, 8, 16 };

foreach (int degree in degrees)
{
    stopwatch.Restart();
    var r = numbers
        .AsParallel()
        .WithDegreeOfParallelism(degree)
        .Select(HeavyCalculation)
        .ToList();
    stopwatch.Stop();
    long time = stopwatch.ElapsedMilliseconds;
    Console.WriteLine("Degree " + degree + ": " + time + " ms, Speedup: " + Math.Round((double)linqTime / time, 2) + "x");
}

Console.WriteLine();
Console.WriteLine("Order");
Console.WriteLine();

var smallNumbers = Enumerable.Range(1, 100);

var unordered = smallNumbers
    .AsParallel()
    .Select(x => x)
    .ToList();

Console.WriteLine("Without AsOrdered:");
Console.WriteLine(string.Join(", ", unordered));
Console.WriteLine("Order preserved: " + unordered.SequenceEqual(smallNumbers));
Console.WriteLine();

var ordered = smallNumbers
    .AsParallel()
    .AsOrdered()
    .Select(x => x)
    .ToList();

Console.WriteLine("With AsOrdered:");
Console.WriteLine(string.Join(", ", ordered));
Console.WriteLine("Order preserved: " + ordered.SequenceEqual(smallNumbers));
Console.WriteLine();

Console.WriteLine("Simple operation");
Console.WriteLine();

stopwatch.Restart();
var simpleLinq = numbers
    .Select(x => x * 2)
    .ToList();
stopwatch.Stop();
long simpleLinqTime = stopwatch.ElapsedMilliseconds;
Console.WriteLine("LINQ:  " + simpleLinqTime + " ms");

stopwatch.Restart();
var simplePlinq = numbers
    .AsParallel()
    .Select(x => x * 2)
    .ToList();
stopwatch.Stop();
long simplePlinqTime = stopwatch.ElapsedMilliseconds;
Console.WriteLine("PLINQ: " + simplePlinqTime + " ms");

if (simplePlinqTime > 0)
{
    Console.WriteLine("Speedup: " + Math.Round((double)simpleLinqTime / simplePlinqTime, 2) + "x");
}

Console.ReadLine();

/*
 * Почему PLINQ может менять порядок?
PLINQ делит коллекцию на части и раздает их разным потокам. Потоки
заканчивают работу в разное время, поэтому результаты могут собраться
не по порядку.

Для чего нужен AsOrdered()?
Он говорит PLINQ, что порядок элементов нужно сохранить таким же,
как в исходной коллекции.

Есть ли цена у сохранения порядка?
Да. Нужно следить за порядком и собирать результаты в правильной
последовательности, поэтому программа работает медленнее и использует
больше памяти.

Почему в первом тесте PLINQ быстрее, а во втором преимущество
маленькое или его нет?
В первом тесте каждый элемент считается долго, поэтому выигрыш от
нескольких ядер больше, чем затраты на параллельность. Во втором тесте
x * 2 считается мгновенно, и время уходит на разбиение данных,
запуск потоков и сбор результатов, поэтому PLINQ может быть таким же
или даже медленнее.

Когда стоит использовать PLINQ?
Когда PLINQ может ускорить программу: когда элементов много и над
каждым выполняется тяжелая вычислительная операция, а элементы не
зависят друг от друга.

Когда PLINQ использовать бессмысленно: когда операция очень простая,
коллекция маленькая или операции зависят друг от друга, а также когда
идет работа с файлами или сетью, где поток просто ждет.

Почему большая коллекция сама по себе не гарантирует ускорение?
Потому что важна не только длина коллекции, а то, сколько работы
приходится на один элемент. Если работа маленькая, то накладные
расходы на параллельность съедают весь выигрыш.

Почему CPU-bound операции хорошие кандидаты?
Потому что они загружают процессор, и если разделить их между
несколькими ядрами, то они посчитаются быстрее.

Как AsOrdered() влияет на выполнение?
Сохраняет порядок результатов, но замедляет выполнение, так как
PLINQ тратит время на упорядочивание.

Зачем нужен WithDegreeOfParallelism()?
Чтобы задать, сколько потоков одновременно могут работать над
запросом. Так можно не занимать все ядра процессора или подобрать
лучшее количество потоков для задачи.
*/