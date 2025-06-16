using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.Services;
using AICalendar.Domain.ValueObjects;
using Moq;

namespace AICalendar.Domain.Tests.Services;

public class SchedulingServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly SchedulingService _schedulingService;
    private readonly User _user1;
    private readonly User _user2;

    public SchedulingServiceTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _schedulingService = new SchedulingService(_eventRepositoryMock.Object);
        
        _user1 = new User(Guid.Parse("11111111-1111-1111-1111-111111111111"), "User 1", "user1@example.com");
        _user2 = new User(Guid.Parse("22222222-2222-2222-2222-222222222222"), "User 2", "user2@example.com");
    }

    [Fact]
    public void FindAvailableSlots_WithNoEvents_ReturnsEntireRange()
    {
        // Arrange
        var users = new List<User> { _user1, _user2 };
        var startDate = new DateTime(2023, 5, 1, 9, 0, 0);
        var endDate = startDate.AddHours(8); // 9 AM to 5 PM
        var searchRange = new DateTimeRange(startDate, endDate);
        var duration = TimeSpan.FromHours(1);
        
        _eventRepositoryMock
            .Setup(repo => repo.GetByUserIdAndTimeRangeAsync(
                It.IsAny<Guid>(), 
                It.IsAny<DateTimeRange>(), 
                It.IsAny<EventStatus>()))
            .ReturnsAsync(new List<Event>());

        // Act
        var result = _schedulingService.FindAvailableSlots(users, searchRange, duration);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        // Should have slots every 30 minutes (9:00, 9:30, 10:00, etc.) 
        // For an 8-hour day with 30-min intervals, that's 15 slots (8 hours * 2 slots per hour - 1)
        Assert.Equal(15, result.Value.Count);
        Assert.Equal(startDate, result.Value[0].Start);
        Assert.Equal(duration, result.Value[0].Duration);
    }

    [Fact]
    public void FindAvailableSlots_WithBusyPeriods_ReturnsAvailableSlots()
    {
        // Arrange
        var users = new List<User> { _user1, _user2 };
        var startDate = new DateTime(2023, 5, 1, 9, 0, 0);
        var endDate = startDate.AddHours(8); // 9 AM to 5 PM
        var searchRange = new DateTimeRange(startDate, endDate);
        var duration = TimeSpan.FromHours(1);
        
        // User 1 has a meeting from 10 AM to 11 AM
        var user1Event = new Event(
            "Meeting 1", 
            new DateTimeRange(startDate.AddHours(1), startDate.AddHours(2)), 
            _user1, 
            "Test meeting");
            
        // User 2 has a meeting from 2 PM to 3 PM
        var user2Event = new Event(
            "Meeting 2", 
            new DateTimeRange(startDate.AddHours(5), startDate.AddHours(6)), 
            _user2, 
            "Another test meeting");
        
        _eventRepositoryMock
            .Setup(repo => repo.GetByUserIdAndTimeRangeAsync(
                _user1.Id, 
                It.IsAny<DateTimeRange>(), 
                It.IsAny<EventStatus>()))
            .ReturnsAsync(new List<Event> { user1Event });
            
        _eventRepositoryMock
            .Setup(repo => repo.GetByUserIdAndTimeRangeAsync(
                _user2.Id, 
                It.IsAny<DateTimeRange>(), 
                It.IsAny<EventStatus>()))
            .ReturnsAsync(new List<Event> { user2Event });

        // Act
        var result = _schedulingService.FindAvailableSlots(users, searchRange, duration);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        
        // Verify no slots during the busy periods
        foreach (var slot in result.Value)
        {
            Assert.False(slot.Overlaps(user1Event.TimeRange));
            Assert.False(slot.Overlaps(user2Event.TimeRange));
        }
    }

    [Fact]
    public void FindAvailableSlots_WithOverlappingEvents_MergesBusyPeriods()
    {
        // Arrange
        var users = new List<User> { _user1 };
        var startDate = new DateTime(2023, 5, 1, 9, 0, 0);
        var endDate = startDate.AddHours(8); // 9 AM to 5 PM
        var searchRange = new DateTimeRange(startDate, endDate);
        var duration = TimeSpan.FromHours(1);
        
        // User has two overlapping meetings
        var meeting1 = new Event(
            "Meeting 1", 
            new DateTimeRange(startDate.AddHours(1), startDate.AddHours(3)), // 10 AM to 12 PM
            _user1);
            
        var meeting2 = new Event(
            "Meeting 2", 
            new DateTimeRange(startDate.AddHours(2), startDate.AddHours(4)), // 11 AM to 1 PM
            _user1);
        
        _eventRepositoryMock
            .Setup(repo => repo.GetByUserIdAndTimeRangeAsync(
                _user1.Id, 
                It.IsAny<DateTimeRange>(), 
                It.IsAny<EventStatus>()))
            .ReturnsAsync(new List<Event> { meeting1, meeting2 });

        // Act
        var result = _schedulingService.FindAvailableSlots(users, searchRange, duration);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        
        // Verify no slots between 10 AM and 1 PM (merged busy period)
        var mergedBusyPeriod = new DateTimeRange(startDate.AddHours(1), startDate.AddHours(4));
        foreach (var slot in result.Value)
        {
            Assert.False(slot.Overlaps(mergedBusyPeriod));
        }
    }

    [Fact]
    public void FindAvailableSlots_WithNonWorkingHours_RespectsSearchRange()
    {
        // Arrange
        var users = new List<User> { _user1 };
        
        // Search range is only 10 AM to 2 PM
        var startDate = new DateTime(2023, 5, 1, 10, 0, 0);
        var endDate = startDate.AddHours(4);
        var searchRange = new DateTimeRange(startDate, endDate);
        var duration = TimeSpan.FromHours(1);
        
        _eventRepositoryMock
            .Setup(repo => repo.GetByUserIdAndTimeRangeAsync(
                It.IsAny<Guid>(), 
                It.IsAny<DateTimeRange>(), 
                It.IsAny<EventStatus>()))
            .ReturnsAsync(new List<Event>());

        // Act
        var result = _schedulingService.FindAvailableSlots(users, searchRange, duration);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        
        // All slots should be within the search range
        foreach (var slot in result.Value)
        {
            Assert.True(slot.Start >= searchRange.Start);
            Assert.True(slot.End <= searchRange.End);
        }
    }

    [Fact]
    public void FindAvailableSlots_WithInvalidParameters_ReturnsFailure()
    {
        // Arrange
        var users = new List<User>();
        var startDate = DateTime.Now;
        var endDate = startDate.AddHours(8);
        var searchRange = new DateTimeRange(startDate, endDate);
        var duration = TimeSpan.FromHours(1);

        // Act & Assert - No users
        var resultNoUsers = _schedulingService.FindAvailableSlots(users, searchRange, duration);
        Assert.True(resultNoUsers.IsFailure);
        Assert.Contains("At least one user must be specified", resultNoUsers.Error);

        // Act & Assert - Duration too long
        var resultDurationTooLong = _schedulingService.FindAvailableSlots(
            new List<User> { _user1 }, 
            searchRange, 
            TimeSpan.FromHours(10));
        Assert.True(resultDurationTooLong.IsFailure);
        Assert.Contains("Duration cannot be longer than search range", resultDurationTooLong.Error);

        // Act & Assert - Invalid duration
        var resultInvalidDuration = _schedulingService.FindAvailableSlots(
            new List<User> { _user1 }, 
            searchRange, 
            TimeSpan.Zero);
        Assert.True(resultInvalidDuration.IsFailure);
        Assert.Contains("Duration must be greater than zero", resultInvalidDuration.Error);
    }
}