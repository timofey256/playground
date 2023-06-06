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

	public override void SayHello()
	{
		Console.WriteLine($"Ahoj, jsem {_name}");
	}
	
	public override void SayBye()
	{
		Console.WriteLine("Čau");
	}
}

class German : Person
{
	public German(string name) : base(name) {}

	public override void SayHello()
	{
		Console.WriteLine($"Hallo, ich bin {_name}");
	}

	public new void SayBye()
	{
		Console.WriteLine("Tschuss");
	}
}

class MainClass
{
	public static void Main(string[] args)
	{
		Person[] people = new Person[] { new Czech("Martin"), new German("Wolfgang")};

		// How to use virtual functions for polymorfism:
		foreach(Person p in people)
		{
			p.SayHello();
			p.SayBye();
		}
		
		// `new` and `override` keywords here work the same:
		Czech p1 = new Czech("Martin");
		p1.SayBye();
		German p2 = new German("Wolfgang");
		p2.SayBye();
		
		/* Difference between `new` and `override` keywords.
		 * 
		 * Output:
		 * ----
		 * Čau
		 * Bye from person
		 * ----
		 * `override` keyword overrides functions even in the parent class, whereas `new` doesn't do it. 
		 * 
		 * */
		Person p1_Czech = (Person)p1;
		p1_Czech.SayBye();
		Person p2_German = (Person)p2;
		p2_German.SayBye();
	}
}
