namespace AI;

public class Machine
{
    //Pointer manipulation
    const byte IncrementMemoryPointer = 1;
    
    //Information manipulation
    const byte IncrementMemoryValue = 2;

    //Communication
    const byte ReadInput = 3;
    const byte WriteOutput = 4;

    //Control flow
    const byte JumpForward4 = 5;
    const byte JumpBackward4 = 6;
    const byte JumpForward16 = 7;
    const byte JumpBackward16 = 8;
    const byte JumpForward64 = 9;
    const byte JumpBackward64 = 10;
    const byte JumpForward256 = 11;
    const byte JumpBackward256 = 12;
    
    //Halting
    const byte Return = 13;

    public const byte MaximumInstruction = 14;

    public struct ExecutionResult
    {
        public long Cycles;
        public long InputBytesRead;
    }

    public static ExecutionResult Execute(
        Span<byte> program, 
        Span<byte> memory,
        Span<byte> input,
        Span<byte> output,
        long cycleLimit)
    {
        var programCounter = 0;
        var memoryPointer = 0;
        var inputPointer = 0;
        var outputPointer = 0;
        var cycles = 0;

        while (true)
        {
            if (cycles > cycleLimit) { break; }

            switch (program[programCounter])
            {
                case IncrementMemoryPointer:
                    memoryPointer++;
                    memoryPointer %= memory.Length;
                    break;
                case IncrementMemoryValue:
                    memory[memoryPointer]++;
                    break;
                case ReadInput:
                    if (inputPointer < input.Length - 1)
                    {
                        memory[memoryPointer] = input[inputPointer++];
                    }
                    break;
                case WriteOutput:
                    if (outputPointer < output.Length - 1)
                    {
                        output[outputPointer++] = memory[memoryPointer];
                    }
                    break;
                case JumpForward4:
                    programCounter += 4;
                    break;
                case JumpForward16:
                    programCounter += 16;
                    break;
                case JumpForward64:
                    programCounter += 64;
                    break;
                case JumpForward256:
                    programCounter += 256;
                    break;
                case JumpBackward4:
                    programCounter = (programCounter - 4 + program.Length) % program.Length;
                    break;
                case JumpBackward16:
                    programCounter = (programCounter - 16 + program.Length) % program.Length;
                    break;
                case JumpBackward64:
                    programCounter = (programCounter - 64 + program.Length) % program.Length;
                    break;
                case JumpBackward256:
                    programCounter = (programCounter - 256 + program.Length) % program.Length;
                    break;
                case Return:
                   return new ExecutionResult
                   {
                       Cycles = cycles,
                       InputBytesRead = inputPointer
                   };
            }

            cycles++;
            programCounter++;
            programCounter %= program.Length;
        }

        return new ExecutionResult
        {
            Cycles = cycles,
            InputBytesRead = inputPointer
        };
    }
}
