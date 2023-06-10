namespace InheritanceExamples;

class Person
{
	protected string _name { get; }

	public Person(string name)
	{
		_name = name;
	}

	public virtual void SayHello()
	{
		Console.WriteLine($"Hello, I am a person called {_name}");
	}

	public virtual void SayBye()
	{
		Console.WriteLine("Bye from person.");
	}
}


class Czech : Person
{
	public Czech(string name) : base(name) {}

	public new void SayHello()
	{
		Console.WriteLine($"Ahoj, jsem {_name}");
	}

	public void SayHello2()
	{
		SayHello();
		SayHello();
	}
	
	public override void SayBye()
	{
		Console.WriteLine("Čau");
	}
}

class MainClass
{
	public static void Main(string[] args)
	{
		Czech p1 = new Czech("Martin");
		p1.SayHello2();	
	}
}
