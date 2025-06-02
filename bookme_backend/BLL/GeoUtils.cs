using bookme_backend.DataAcces.DTO;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

public static class GeoUtils
{ 
    public static double? CalcularDistanciaEnKilometros(Ubicacion ubicacion1, Ubicacion ubicacion2)
    {
        if (ubicacion1?.Latitud == null || ubicacion1?.Longitud == null ||
            ubicacion2?.Latitud == null || ubicacion2?.Longitud == null)
            return null;

        // Crear sistema de coordenadas
        var wgs84 = GeographicCoordinateSystem.WGS84;
        var webMercator = ProjectedCoordinateSystem.WebMercator;

        // Crear transformador
        var transformFactory = new CoordinateTransformationFactory();
        var transform = transformFactory.CreateFromCoordinateSystems(wgs84, webMercator);

        // Coordenadas WGS84 (lat/lon → lon, lat)
        var coord1 = new[] { ubicacion1.Longitud.Value, ubicacion1.Latitud.Value };
        var coord2 = new[] { ubicacion2.Longitud.Value, ubicacion2.Latitud.Value };

        // Transformar a Web Mercator (metros)
        var punto1 = transform.MathTransform.Transform(coord1);
        var punto2 = transform.MathTransform.Transform(coord2);

        // Calcular distancia en metros y devolver en km
        var distanciaMetros = Math.Sqrt(
            Math.Pow(punto1[0] - punto2[0], 2) +
            Math.Pow(punto1[1] - punto2[1], 2)
        );

        return Math.Round(distanciaMetros / 1000, 3); // km con 2 decimales
    }
}