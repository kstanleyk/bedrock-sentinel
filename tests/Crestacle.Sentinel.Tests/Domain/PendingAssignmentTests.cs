using Crestacle.Sentinel.Core.Entities;
using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Tests.Domain;

public sealed class PendingAssignmentTests
{
    private static PendingAssignment Build(int expiryHours = 72)
        => PendingAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), "actor-a", expiryHours: expiryHours);

    [Fact]
    public void Create_SetsStatusToPending()
    {
        var pa = Build();
        pa.Status.Should().Be(AssignmentStatus.Pending);
    }

    [Fact]
    public void Approve_ByDifferentActor_Succeeds()
    {
        var pa = Build();
        var result = pa.Approve("actor-b");

        result.Should().BeTrue();
        pa.Status.Should().Be(AssignmentStatus.Approved);
        pa.ReviewedBy.Should().Be("actor-b");
        pa.ReviewedOn.Should().NotBeNull();
    }

    [Fact]
    public void Approve_Twice_ReturnsFalse()
    {
        var pa = Build();
        pa.Approve("actor-b");

        var result = pa.Approve("actor-c");
        result.Should().BeFalse();
        pa.ReviewedBy.Should().Be("actor-b"); // unchanged
    }

    [Fact]
    public void Approve_WhenExpired_ReturnsFalse()
    {
        // Create with -1 hour expiry so it's already expired.
        var pa = PendingAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), "actor-a", expiryHours: -1);

        var result = pa.Approve("actor-b");
        result.Should().BeFalse();
        pa.Status.Should().Be(AssignmentStatus.Pending); // unchanged
    }

    [Fact]
    public void Reject_ByDifferentActor_WithReason_Succeeds()
    {
        var pa = Build();
        var result = pa.Reject("actor-b", "Does not qualify.");

        result.Should().BeTrue();
        pa.Status.Should().Be(AssignmentStatus.Rejected);
        pa.ReviewedBy.Should().Be("actor-b");
        pa.RejectionReason.Should().Be("Does not qualify.");
    }

    [Fact]
    public void Reject_AfterApproval_ReturnsFalse()
    {
        var pa = Build();
        pa.Approve("actor-b");

        var result = pa.Reject("actor-c", "Too late.");
        result.Should().BeFalse();
        pa.Status.Should().Be(AssignmentStatus.Approved); // unchanged
    }

    [Fact]
    public void MarkExpired_WhenPending_Succeeds()
    {
        var pa = Build();
        var result = pa.MarkExpired();

        result.Should().BeTrue();
        pa.Status.Should().Be(AssignmentStatus.Expired);
    }

    [Fact]
    public void MarkExpired_WhenAlreadyApproved_ReturnsFalse()
    {
        var pa = Build();
        pa.Approve("actor-b");

        var result = pa.MarkExpired();
        result.Should().BeFalse();
        pa.Status.Should().Be(AssignmentStatus.Approved); // unchanged
    }
}
