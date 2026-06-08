using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<PlateLibContext>(
    connectionName: "postgresdb");

// S3/MinIO client for development file seeding
var serviceUrl = builder.Configuration["Storage:ServiceUrl"];
var accessKey = builder.Configuration["Storage:AccessKey"] ?? string.Empty;
var secretKey = builder.Configuration["Storage:SecretKey"] ?? string.Empty;
var forcePathStyle = builder.Configuration.GetValue<bool>("Storage:ForcePathStyle");
var bucketName = builder.Configuration["Storage:BucketName"] ?? "platelib";

var s3Config = new AmazonS3Config { ForcePathStyle = forcePathStyle };
if (!string.IsNullOrEmpty(serviceUrl))
    s3Config.ServiceURL = serviceUrl;
else
    s3Config.RegionEndpoint = RegionEndpoint.EUCentral1;

builder.Services.AddSingleton<IAmazonS3>(_ =>
    new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config));
builder.Services.AddSingleton(_ => new StorageSeedConfig(bucketName));

var host = builder.Build();
host.Run();
