namespace MyDelegatesExamples;

class Delegates
{
	private delegate void SayStuff(string str);
	
	private static void SayingHelloStuff(string str)
	{
		Console.WriteLine($"Hey! {str}");
	}
	
	private static void SayingByeStuff(string str)
	{
		Console.WriteLine($"{str}. Bye!");
	}
	
	public static void Main(string[] args)
	{
		SayStuff sayStuff = new SayStuff(SayingByeStuff);
		sayStuff("I am 30 y.o");
		
		sayStuff = new SayStuff(SayingByeStuff);
		sayStuff("I am 30 y.o");

		sayStuff += SayingHelloStuff;
		sayStuff("Sky is blue");
	}
}
