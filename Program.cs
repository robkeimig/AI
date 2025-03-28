using System.Diagnostics;
using AI;

const int RandomSeed = 12345;
const int PopulationSize = 10_000;
const int ProgramSize = 4096;
const int MemorySize = 4096;
const int ContextSize = 4096;
const int CycleLimit = 100_000;

var random = new Random(RandomSeed);
var webServer = new WebServer();
var memory = new byte[ContextSize];
var output = new byte[ContextSize];
var nextProgramA = new byte[ProgramSize];
var nextProgramB = new byte[ProgramSize];
var candidates = new List<Candidate>(PopulationSize);
var historicalIterations = new List<long>();
var historicalFitness = new List<double>();
var scoringStopwatch = new Stopwatch();
var iterations = 0L;

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
    Buffer.BlockCopy(candidateA.Program, 0, nextProgramA, 0, ProgramSize);
    Machine.Execute(candidateA.Program, nextProgramA, memory, trainingObjective.Input, output, CycleLimit);
    var candidateATrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateAScore = Evaluation.CommonPrefixLength(trainingObjective.Output, candidateATrimmedOutput);

    Array.Clear(memory);
    Array.Clear(output);
    Buffer.BlockCopy(candidateB.Program, 0, nextProgramB, 0, ProgramSize);
    Machine.Execute(candidateB.Program, nextProgramB, memory, trainingObjective.Input, output, CycleLimit);
    var candidateBTrimmedOutput = new Span<byte>(output, 0, trainingObjective.Output.Length);
    var candidateBScore = Evaluation.CommonPrefixLength(trainingObjective.Output, candidateBTrimmedOutput);

    //Happy cases - Definitive difference in performance between candidates.
    //In both, the winner's next program overwrites the loser's current program.
    if (candidateAScore > candidateBScore) 
    {
        candidateA.ConsecutiveWins++;
        candidateB.ConsecutiveWins = 0;
        Buffer.BlockCopy(nextProgramA, 0, candidateB.Program, 0, ProgramSize);
        //Console.Write('A');
    }
    else if (candidateBScore > candidateAScore)
    {
        candidateB.ConsecutiveWins++;
        candidateA.ConsecutiveWins = 0;
        Buffer.BlockCopy(nextProgramB, 0, candidateA.Program, 0, ProgramSize);
        //Console.Write('B');
    }
    
    ////Unhappy case - No distinguishing behavior. Mutate current program(s):
    //else
    //{
    //    if (candidateA.ConsecutiveWins > candidateB.ConsecutiveWins) //Prefer to mutate the program with fewer consecutive wins.
    //    {
    //        candidateB.Program[random.Next(ProgramSize)] = (byte)random.Next(Machine.MaximumInstruction);
    //        Console.Write('C');
    //    }
    //    else if (candidateB.ConsecutiveWins > candidateA.ConsecutiveWins)
    //    {
    //        candidateA.Program[random.Next(ProgramSize)] = (byte)random.Next(Machine.MaximumInstruction);
    //        Console.Write('D');
    //    }
    //    else
    //    {
    //        candidateA.Program[random.Next(ProgramSize)] = (byte)random.Next(Machine.MaximumInstruction);
    //        candidateB.Program[random.Next(ProgramSize)] = (byte)random.Next(Machine.MaximumInstruction);
    //        Console.Write('E');
    //    }
    //}

    iterations++;
}

void ScoreCandidates()
{
    var scoreIterations = 1000;
    var total = 0d;
    var correct = 0d;

    for (int x = 0; x < scoreIterations; x++)
    {
        var candidate = candidates[random.Next(PopulationSize)];
        var scoringObjective = Evaluation.GetObjective(random);

        Array.Clear(memory);
        Array.Clear(output);
        Buffer.BlockCopy(candidate.Program, 0, nextProgramA, 0, ProgramSize);
        Machine.Execute(candidate.Program, nextProgramA, memory, scoringObjective.Input, output, CycleLimit);
        var trimmedOutput = new Span<byte>(output, 0, scoringObjective.Output.Length);
        var cpl = Evaluation.CommonPrefixLength(scoringObjective.Output, trimmedOutput);
        total += trimmedOutput.Length;
        correct += cpl;
    }

    var score = 1d * correct / total;
    Console.WriteLine(score);

    historicalIterations.Add(iterations);
    historicalFitness.Add(score);

    ScottPlot.Plot myPlot = new();
    myPlot.Axes.SetLimits(top: 1, bottom: 0);
    myPlot.Add.ScatterPoints(historicalIterations, historicalFitness);
    myPlot.XLabel("Generations", size: 24);
    myPlot.YLabel("Fitness", size: 24);

    try
    {
        myPlot.SavePng(@"D:\training_status.png", 1280, 720);
        var plotBytes = File.ReadAllBytes(@"D:\training_status.png");
        webServer.SendImageUpdate(plotBytes);
    }
    catch { }
}