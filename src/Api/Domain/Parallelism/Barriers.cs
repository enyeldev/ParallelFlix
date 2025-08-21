using System;

namespace Api.Domain.Parallel;

public class PipelineBarriers
{
    public Barrier PreprocessBarrier { get; } = new(participantCount: 2);
    public Barrier SimilarityBarrier { get; } = new(participantCount: 2);
}
