namespace Slix
{
    public struct Actions
    {
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Accelerate { get; set; }
        public bool Brake { get; set; }
        public bool HandBrake { get; set; }

        public void Reset()
        {
            Left = false;
            Right = false;
            Accelerate = false;
            Brake = false;
            HandBrake = false;
        }
    }
}
