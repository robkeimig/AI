namespace AI;

public class Machine
{
    //Pointer manipulation
    const byte IncrementMemoryPointer = 1;
    const byte IncrementNextProgramPointer = 2;
    
    //Information manipulation
    const byte IncrementMemoryValue = 3;

    //Communication
    const byte ReadInput = 4;
    const byte WriteOutput = 5;
    const byte CopyNextProgramToMemory = 6;
    const byte CopyMemoryToNextProgram = 7;

    //Control flow
    const byte JumpForward4 = 8;
    const byte JumpBackward4 = 9;
    const byte JumpForward16 = 10;
    const byte JumpBackward16 = 11;
    const byte JumpForward64 = 12;
    const byte JumpBackward64 = 13;
    const byte JumpForward256 = 14;
    const byte JumpBackward256 = 15;
    
    //Halting
    const byte Return = 16;

    public const byte MaximumInstruction = 17;

    public static void Execute(
        Span<byte> program, 
        Span<byte> nextProgram,
        Span<byte> memory,
        Span<byte> input,
        Span<byte> output,
        long cycleLimit)
    {
        var programCounter = 0;
        var nextProgramPointer = 0;
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
                case IncrementNextProgramPointer:
                    nextProgramPointer++;
                    nextProgramPointer %= nextProgram.Length;
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
                case CopyMemoryToNextProgram:
                    nextProgram[nextProgramPointer] = memory[memoryPointer];
                    break;
                case CopyNextProgramToMemory:
                    memory[memoryPointer] = nextProgram[nextProgramPointer];
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
                   return;
            }

            cycles++;
            programCounter++;
            programCounter %= program.Length;
        }
    }
}
