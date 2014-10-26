using AU.DocumentGenerationService.Messages.GenerateCommunication;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public interface ICommandQueue<T> where T : GenerateCommunitcation
    {
        void Enqueue(T command);
        T Dequeue();
        int Count { get; }
    }
}