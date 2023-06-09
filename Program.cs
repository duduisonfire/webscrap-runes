using System.Diagnostics;
using webscrap_runes.webscrap;

WebScrap WebScrap = new("ahri", "mid");
Stopwatch stopwatch = new();

stopwatch.Start();
await WebScrap.GetRunes();
stopwatch.Stop();

Console.WriteLine(stopwatch.Elapsed);
