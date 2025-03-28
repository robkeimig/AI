using System.Diagnostics;
using AI;

const int RandomSeed = 12345;
const int PopulationSize = 100_000;
const int ProgramSize = 4096;
const int MemorySize = 4096;
const int ContextSize = 4096;
const int CycleLimit = 100_000;

var random = new Random(RandomSeed); 
var webServer = new WebServer();
var memory = new byte[MemorySize];
var output = new byte[ContextSize];
var nextProgram = new byte[ProgramSize];
var nextProgram2 = new byte[ProgramSize];
var candidates = new List<Candidate>(PopulationSize);
var historicalTournaments = new List<long>();
var historicalFitness = new List<double>();
var scoringStopwatch = new Stopwatch();
var tournaments = 0L;

for (int x = 0; x < PopulationSize; x++)
{
    var program = new byte[ProgramSize];

    for (int y = 0; y < program.Length; y++)
    {
        program[y] = (byte)random.Next(Machine.MaximumInstruction);
    }

    candidates.Add(new Candidate
    {
        Program = program
    });
}

scoringStopwatch.Start();

while (true)
{
    if (scoringStopwatch.ElapsedMilliseconds > 1_000)
    {
        scoringStopwatch.Restart();
        ScoreCandidates();
    }

    var trainingObjective = Evaluation.GetObjective(random);
    Candidate candidateA = candidates[random.Next(PopulationSize)];
    Candidate? candidateB = default;

    while (candidateB == null || candidateA == candidateB)
    {
        candidateB = candidates[random.Next(PopulationSize)];
    }

    Array.Clear(memory);
    Array.Clear(output);
    Array.Clear(nextProgram);
    var executionResultA = Machine.Execute(candidateA.Program, nextProgram, memory, trainingObjective.Input, output, CycleLimit);
    var candidateATrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateAScore = Evaluation.CommonPrefixLength(trainingObjective.Output, candidateATrimmedOutput);

    Array.Clear(memory);
    Array.Clear(output);
    Array.Clear(nextProgram2);
    var executionResultB = Machine.Execute(candidateB.Program, nextProgram2, memory, trainingObjective.Input, output, CycleLimit);
    var candidateBTrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateBScore = Evaluation.CommonPrefixLength(trainingObjective.Output, candidateBTrimmedOutput);

    if (candidateAScore > candidateBScore)
    {
        candidateA.ConsecutiveWins++;
        candidateB.ConsecutiveWins = 0;
        Buffer.BlockCopy(nextProgram, 0, candidateB.Program, 0, ProgramSize);
    }
    else if (candidateBScore > candidateAScore)
    {
        candidateB.ConsecutiveWins++;
        candidateA.ConsecutiveWins = 0;
        Buffer.BlockCopy(nextProgram2, 0, candidateA.Program, 0, ProgramSize);
    }

    tournaments++;
}

void ScoreCandidates()
{
    var scoreIterations = 100;
    var total = 0L;
    var correct = 0L;

    for (int x = 0; x < scoreIterations; x++)
    {
        var candidate = candidates[random.Next(PopulationSize)];
        var scoringObjective = Evaluation.GetObjective(random);
        Array.Clear(memory);
        Array.Clear(output);
        Array.Clear(nextProgram);
        var executionResult = Machine.Execute(candidate.Program, nextProgram, memory, scoringObjective.Input, output, CycleLimit);
        var trimmedOutput = new Span<byte>(output, 0, scoringObjective.Output.Length);
        total += scoringObjective.Output.Length;
        correct += Evaluation.CommonPrefixLength(scoringObjective.Output, trimmedOutput);
    }

    var score = 1d * correct / total;
    Console.WriteLine(score);

    historicalTournaments.Add(tournaments);
    historicalFitness.Add(score);

    ScottPlot.Plot myPlot = new();
    myPlot.Axes.SetLimits(top: 1, bottom: 0);
    myPlot.Add.ScatterPoints(historicalTournaments, historicalFitness);
    myPlot.XLabel("Tournaments", size: 24);
    myPlot.YLabel("Fitness", size: 24);

    try
    {
        myPlot.SavePng(@"D:\training_status.png", 1280, 720);
        var plotBytes = File.ReadAllBytes(@"D:\training_status.png");
        webServer.SendImageUpdate(plotBytes);
    }
    catch { }
}