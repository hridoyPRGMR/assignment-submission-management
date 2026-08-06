using AssignmentSystem.Domain.Common;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_should_have_no_error()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_should_carry_error_message()
    {
        var result = Result.Failure("something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Generic_success_should_expose_value()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Generic_failure_should_throw_when_accessing_value()
    {
        var result = Result.Failure<int>("bad input");

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructing_success_with_error_should_throw()
    {
        var act = () => new TestableResult(true, "should not have an error");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructing_failure_without_error_should_throw()
    {
        var act = () => new TestableResult(false, null);

        act.Should().Throw<InvalidOperationException>();
    }

    private sealed class TestableResult : Result
    {
        public TestableResult(bool isSuccess, string? error) : base(isSuccess, error)
        {
        }
    }
}
