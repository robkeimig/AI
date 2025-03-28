namespace AI;

public class Machine
{
    //Pointer manipulation
    const byte IncrementMemoryPointer = 0;
    const byte DecrementMemoryPointer = 1;
    const byte IncrementNextProgramPointer = 2;
    const byte DecrementNextProgramPointer = 3;
    
    //Information manipulation
    const byte IncrementMemoryValue = 4;
    const byte DecrementMemoryValue = 5;

    //Communication
    const byte ReadInput = 6;
    const byte WriteOutput = 7;
    const byte CopyNextProgramToMemory = 8;
    const byte CopyMemoryToNextProgram = 9;

    //Control flow
    const byte JumpForward4 = 10;
    const byte JumpBackward4 = 11;
    const byte JumpForward16 = 12;
    const byte JumpBackward16 = 13;
    const byte JumpForward64 = 14;
    const byte JumpBackward64 = 15;
    const byte JumpForward256 = 16;
    const byte JumpBackward256 = 17;
    
    //Halting
    const byte Return = 18;

    public const byte MaximumInstruction = 19;

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
            if (programCounter > program.Length - 1) { break; }

            switch (program[programCounter])
            {
                //...
            }

            cycles++;
            programCounter++;
        }
    }
}
