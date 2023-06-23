using System;
using System.Collections.Generic;
using System.Linq;

public class KMeans
{
    private int _k;
    private List<Point> _points;

    public KMeans(int k, List<Point> points)
    {
        _k = k;
        _points = points;
    }

    public List<List<Point>> Cluster()
    {
        // Inizializza i centroidi in modo casuale
        List<Point> centroids = InitializeCentroids();

        // Assegna ciascun punto al cluster più vicino
        List<List<Point>> clusters = AssignPointsToClusters(centroids);

        // Calcola i nuovi centroidi
        List<Point> newCentroids = CalculateNewCentroids(clusters);

        // Continua finché i centroidi non convergono
        while (!CentroidsConverged(centroids, newCentroids))
        {
            centroids = newCentroids;

            clusters = AssignPointsToClusters(centroids);

            newCentroids = CalculateNewCentroids(clusters);
        }

        return clusters;
    }

    public List<Point> GetCentroids(List<List<Point>> clusters)
{
    List<Point> centroids = new List<Point>();

    foreach (List<Point> cluster in clusters)
    {
        double sumX = 0;
        double sumY = 0;

        foreach (Point point in cluster)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        double centroidX = sumX / cluster.Count;
        double centroidY = sumY / cluster.Count;

        Point centroid = new Point(centroidX, centroidY);
        centroids.Add(centroid);
    }

    return centroids;
}

    private List<Point> InitializeCentroids()
    {
        List<Point> centroids = new List<Point>();

        Random random = new Random();

        for (int i = 0; i < _k; i++)
        {
            int index = random.Next(_points.Count);

            centroids.Add(_points[index]);
        }

        return centroids;
    }

    private List<List<Point>> AssignPointsToClusters(List<Point> centroids)
    {
        List<List<Point>> clusters = new List<List<Point>>();

        for (int i = 0; i < _k; i++)
        {
            clusters.Add(new List<Point>());
        }

        foreach (Point point in _points)
        {
            double minDistance = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < _k; i++)
            {
                double distance = point.DistanceTo(centroids[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

            clusters[minIndex].Add(point);
        }

        return clusters;
    }

    private List<Point> CalculateNewCentroids(List<List<Point>> clusters)
    {
        List<Point> centroids = new List<Point>();

        for (int i = 0; i < _k; i++)
        {
            Point centroid = new Point();

            foreach (Point point in clusters[i])
            {
                centroid.X += point.X;
                centroid.Y += point.Y;
            }

            centroid.X /= clusters[i].Count;
            centroid.Y /= clusters[i].Count;

            centroids.Add(centroid);
        }

        return centroids;
    }

    private bool CentroidsConverged(List<Point> oldCentroids, List<Point> newCentroids)
    {
        double epsilon = 0.0001;

        for (int i = 0; i < _k; i++)
        {
            if (oldCentroids[i].DistanceTo(newCentroids[i]) > epsilon)
            {
                return false;
            }
        }

        return true;
    }
}

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }

    public Point()
    {
        X = 0;
        Y = 0;
    }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double DistanceTo(Point other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}