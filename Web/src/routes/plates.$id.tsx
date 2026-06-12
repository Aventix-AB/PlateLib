import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowLeft, ExternalLink, FileDown } from "lucide-react";
import { $api } from "@/lib/api/client";
import { InfoCard } from "@/components/ui/card";
import { EntityThumbnail } from "@/components/ui/thumbnail";

export const Route = createFileRoute("/plates/$id")({
  component: PlateDetailPage,
});

function ManufacturerBadge({
  id,
  name,
  hasThumbnail,
}: {
  id: string;
  name: string;
  hasThumbnail?: boolean;
}) {
  return (
    <Link to="/manufacturers/$id" params={{ id }} className="pl-badge-link">
      <EntityThumbnail
        src={hasThumbnail ? `/api/manufacturers/${id}/thumbnail` : null}
        alt={name}
        size="sm"
        className="w-5 h-5"
      />
      {name}
    </Link>
  );
}

function PlateDetailPage() {
  const { id } = Route.useParams();

  const {
    data: plate,
    isLoading,
    isError,
  } = $api.useQuery("get", "/api/plates/{id}", {
    params: { path: { id } },
  });

  if (isLoading) {
    return (
      <main className="pl-page">
        <p className="text-muted-foreground">Loading…</p>
      </main>
    );
  }

  if (isError || !plate) {
    return (
      <main className="pl-page">
        <Link to="/" className="pl-back-link">
          <ArrowLeft size={16} /> Back to plates
        </Link>
        <p className="text-destructive">Plate not found.</p>
      </main>
    );
  }

  // plate is typed as `never` in current schema — cast until schema is regenerated
  const p = plate as {
    id: string;
    name: string;
    catalogNumber: string;
    productUrl: string;
    wellCount: number;
    material: { code: string; name: string };
    manufacturerId: string;
    manufacturerName: string;
    properties: { name: string; value: string }[];
    files: { id: string; fileName: string; contentType: string }[];
  };

  return (
    <main className="pl-page">
      <Link to="/" className="pl-back-link">
        <ArrowLeft size={16} /> Back to plates
      </Link>

      <h1 className="text-3xl font-bold text-foreground mb-1">{p.name}</h1>
      <p className="text-muted-foreground mb-4 font-mono text-sm">
        {p.catalogNumber}
      </p>
      <a
        href={p.productUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="inline-flex items-center gap-1 text-sm text-primary hover:underline mb-4"
      >
        View product page
        <ExternalLink size={13} />
      </a>

      <div className="mb-8">
        <ManufacturerBadge id={p.manufacturerId} name={p.manufacturerName} />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 mb-8">
        <InfoCard label="Wells" value={String(p.wellCount)} />
        <InfoCard
          label="Material"
          value={`${p.material.code} — ${p.material.name}`}
        />
      </div>

      {p.properties.length > 0 && (
        <section className="mb-8">
          <h2 className="pl-section-heading">Properties</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            {p.properties.map((prop) => (
              <div key={prop.name} className="pl-card pl-card-body">
                <div className="pl-card-label">{prop.name}</div>
                <div className="pl-card-value">{prop.value}</div>
              </div>
            ))}
          </div>
        </section>
      )}

      {p.files.length > 0 && (
        <section>
          <h2 className="pl-section-heading">Files</h2>
          <ul className="space-y-2">
            {p.files.map((f) => (
              <li key={f.id}>
                <a
                  href={`/api/files/${f.id}/download`}
                  download={f.fileName}
                  className="inline-flex items-center gap-2 text-sm text-primary hover:underline"
                >
                  <FileDown size={16} />
                  {f.fileName}
                  <span className="text-muted-foreground text-xs">
                    ({f.contentType})
                  </span>
                </a>
              </li>
            ))}
          </ul>
        </section>
      )}
    </main>
  );
}
