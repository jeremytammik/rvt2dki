namespace rvt2dki
{
  /// <summary>
  /// Net and gross area of a BIM element
  /// </summary>
  internal class ElementArea
  {
    /// <summary>
    /// Constructor taking net area
    /// </summary>
    public ElementArea(long id, string comment, double areaNet)
    {
      Id = id;
      Comment = comment;
      AreaNet = areaNet;
    }
    /// <summary>
    /// Element id
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// BIM element comment
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// Net area, e.g., wall area minus door and window opening areas
    /// </summary>
    public double AreaNet { get; set; }
    /// <summary>
    /// Gross area
    /// </summary>
    public double AreaGross { get; set; }
  }
}
