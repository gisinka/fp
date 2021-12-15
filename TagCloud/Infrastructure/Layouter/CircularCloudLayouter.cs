﻿using System.Drawing;
using TagCloud.App.UI.Console.Common;
using TagCloud.Infrastructure.Monad;

namespace TagCloud.Infrastructure.Layouter;

public class CircularCloudLayouter : ICloudLayouter
{
    private readonly List<Rectangle> rectangles;
    private readonly Spiral spiral;
    public Point Center => spiral.Center;

    public CircularCloudLayouter(IImageSettingsProvider settings) : this(new Point(settings.ImageWidth / 2, settings.ImageHeight / 2)) { }

    public CircularCloudLayouter(Point center)
    {
        rectangles = new List<Rectangle>();
        spiral = new Spiral(center);
    }

    public Result<Rectangle> PutNextRectangle(Size rectangleSize)
    {
        if (rectangleSize.Width <= 0 || rectangleSize.Height <= 0)
            return Result.Fail<Rectangle>("Rectangle sizes must be greater than zero");

        Rectangle rectangle;

        do
        {
            var point = spiral.GetNextPoint() - rectangleSize / 2;
            rectangle = new Rectangle(point, rectangleSize);
        } while (IsLayoutIntersectWith(rectangle));

        rectangle = MoveToCenter(rectangle);

        rectangles.Add(rectangle);

        return Result.Ok(rectangle);
    }

    public Rectangle[] GetLayout()
    {
        return rectangles.ToArray();
    }

    private bool IsLayoutIntersectWith(Rectangle rectangle)
    {
        return rectangles.Any(rectangle.IntersectsWith);
    }

    private Rectangle MoveToCenter(Rectangle rectangle)
    {
        if (rectangles.Count == 0)
            return rectangle;

        rectangle = ShiftToCenter(rectangle, false);
        rectangle = ShiftToCenter(rectangle, true);
        rectangle = ShiftToCenter(rectangle, false);

        return rectangle;
    }

    private Rectangle ShiftToCenter(Rectangle rectangle, bool isVertical)
    {
        var oldDistance = (rectangle.Location + rectangle.Size / 2).GetDistance(spiral.Center);
        var oldLocation = rectangle.Location;

        while (true)
        {
            var newLocation = isVertical
                ? new Point(oldLocation.X,
                    oldLocation.Y + Math.Sign(spiral.Center.Y - rectangle.Location.Y - rectangle.Size.Height / 2))
                : new Point(
                    oldLocation.X + Math.Sign(spiral.Center.X - rectangle.Location.X - rectangle.Size.Width / 2),
                    oldLocation.Y);

            var newDistance = (newLocation + rectangle.Size / 2).GetDistance(spiral.Center);
            var newRectangle = new Rectangle(newLocation, rectangle.Size);

            if (!IsLayoutIntersectWith(newRectangle))
                rectangle = newRectangle;

            if (newDistance >= oldDistance)
                break;

            oldLocation = rectangle.Location;
            oldDistance = newDistance;
        }

        return rectangle;
    }
}