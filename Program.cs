using System.Diagnostics;
using AI;

const int RandomSeed = 12345;
const int PopulationSize = 10_000;
const int ProgramSize = 10_000;
const int MemorySize = 4096;
const int ContextSize = 4096;
const int CycleLimit = 1_000_000;

var random = new Random(RandomSeed); 
var webServer = new WebServer();
var memory = new byte[MemorySize];
var output = new byte[ContextSize];
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
    var executionResultA = Machine.Execute(candidateA.Program, memory, trainingObjective.Input, output, CycleLimit);
    var candidateATrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateAScore = NormalizedScore(trainingObjective.Input.Length, executionResultA.InputBytesRead, trainingObjective.Output, candidateATrimmedOutput, executionResultA.OutputBytesWritten);

    Array.Clear(memory);
    Array.Clear(output);
    var executionResultB = Machine.Execute(candidateB.Program, memory, trainingObjective.Input, output, CycleLimit);
    var candidateBTrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateBScore = NormalizedScore(trainingObjective.Input.Length, executionResultB.InputBytesRead, trainingObjective.Output, candidateBTrimmedOutput, executionResultB.OutputBytesWritten);

    if (candidateAScore > candidateBScore)
    {
        candidateA.ConsecutiveWins++;
        candidateB.ConsecutiveWins = 0;
        Buffer.BlockCopy(candidateA.Program, 0, candidateB.Program, 0, ProgramSize);
        Mutate(candidateB.Program);
        
    }
    else if (candidateBScore > candidateAScore)
    {
        candidateB.ConsecutiveWins++;
        candidateA.ConsecutiveWins = 0;
        Buffer.BlockCopy(candidateB.Program, 0, candidateA.Program, 0, ProgramSize);
        Mutate(candidateA.Program);
    }
    else
    {
        Mutate(candidateA.Program);
        Mutate(candidateB.Program);
    }

    tournaments++;
}

void Mutate(byte[] program)
{
    program[random.Next(ProgramSize)] = (byte)random.Next(Machine.MaximumInstruction);
}

double NormalizedScore(int inputLength, int inputBytesRead, Span<byte> expectedOutput, Span<byte> output, int outputBytesWritten)
{
    var cpl = 1f * Evaluation.CommonPrefixLength(expectedOutput, output) / output.Length;
    //var hd = 1f - Evaluation.HammingDistance(expectedOutput, output);
    var inputScore = 1f * inputBytesRead / inputLength;
    var outputScore = Math.Min(1f * outputBytesWritten / output.Length, 1f);
    return /*hd * .4f + */cpl * .8f + inputScore * .1f + outputScore * .1f;
}

void ScoreCandidates()
{
    var scoreIterations = 100;
    var total = 0d;
    var correct = 0d;

    for (int x = 0; x < scoreIterations; x++)
    {
        var candidate = candidates[random.Next(PopulationSize)];
        var scoringObjective = Evaluation.GetObjective(random);
        Array.Clear(memory);
        Array.Clear(output);
        var executionResult = Machine.Execute(candidate.Program, memory, scoringObjective.Input, output, CycleLimit);
        var trimmedOutput = new Span<byte>(output, 0, scoringObjective.Output.Length);
        total += 1f;
        correct += NormalizedScore(scoringObjective.Input.Length, executionResult.InputBytesRead, scoringObjective.Output, trimmedOutput, executionResult.OutputBytesWritten);
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