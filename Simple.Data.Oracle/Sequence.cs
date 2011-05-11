namespace Simple.Data.Oracle
{
    public class CurrentSequence : Sequence
    {
        public CurrentSequence(string sequenceName) : base(sequenceName) { }

        public override string ToString()
        {
            return SequenceName + ".CURRVAL";
        }
    }

    public class NextSequence : Sequence
    {
        public NextSequence(string sequenceName) : base(sequenceName) { }

        public override string ToString()
        {
            return SequenceName + ".NEXTVAL";
        }
    }

    public abstract class Sequence
    {
        private readonly string _sequenceName;

        protected Sequence(string sequenceName)
        {
            _sequenceName = sequenceName;
        }

        public string SequenceName
        {
            get { return _sequenceName; }
        }

        public static Sequence Next(string sequenceName)
        {
            return new NextSequence(sequenceName);
        }

        public static Sequence Current(string sequenceName)
        {
            return new CurrentSequence(sequenceName);
        }

        public abstract override string ToString();
    }
}