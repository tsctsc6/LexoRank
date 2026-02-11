// See https://aka.ms/new-console-template for more information

using LexoRank.Core;

var lexoRankManager = new LexoRankManager("0123456789");

var sortedList = new SortedDictionary<string, string>();
var sortedValue = string.Empty;
for (var i = 0; i < 10; i++)
{
    // sortedValue = lexoRankManager.Between(sortedValue, string.Empty);
    sortedValue = lexoRankManager.Between(string.Empty, sortedValue);
    sortedList.Add(sortedValue, $"item{i}");
}

Console.WriteLine(string.Join(Environment.NewLine, sortedList));
