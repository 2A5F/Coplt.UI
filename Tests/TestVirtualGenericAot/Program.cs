
var types = new Of[]
{
    new Of<int>(),
    new Of<string>(),
    new Of<float>(),
    new Of<double>(),
}.OrderBy(a => Random.Shared.Next()).ToArray();

var a = types[0].Create();
for (var i = 1; i < types.Length; i++)
{
    a = types[i].Chain(a);
}

Console.WriteLine($"{a}");
