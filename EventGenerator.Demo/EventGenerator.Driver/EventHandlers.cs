namespace EventGenerator.Driver;

using System;

class EventHandlers 
{
    public readonly EventHandler<Foo> FooUpdated;

    public readonly EventHandler<Bar> BarUpdated;

    public class Foo
    {
    }

    public class Bar
    {
    }
}
