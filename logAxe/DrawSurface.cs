//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Drawing;


namespace logAxe
{
   public class DrawSurface
   {
      public Graphics gc;
      public Bitmap bmp;
      public bool SetSize(Size size)
      {
         if (size.Width == 0 || size.Height == 0) return false;
         if (bmp != null && bmp.Size == size) return false;

         bmp = new Bitmap(size.Width, size.Height);
         gc = Graphics.FromImage(bmp);
         return true;
      }
   }
}
