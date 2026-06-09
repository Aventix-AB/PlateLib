import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowLeft, ExternalLink, FileDown } from "lucide-react";
import { $api } from "@/lib/api/client";
import { EntityThumbnail } from "@/components/ui/thumbnail";
import { InfoCard } from "@/components/ui/card";

// These types will be replaced by generated schema types after `pnpm generate:api`
interface PlateItem {
  id: string;
  name: string;
  catalogNumber: string;
  wellCount: number;
}

interface ManufacturerDetail {
  id: string;
  name: string;
  websiteUrl?: string | null;
  hasThumbnail: boolean;
  plates: PlateItem[];
}

interface FileItem {
  id: string;
  fileName: string;
  contentType: string;
}

export const Route = createFileRoute("/manufacturers/$id")({
  component: ManufacturerDetailPage,
});

function ManufacturerDetailPage() {
  const { id } = Route.useParams();

  const {
    data: manufacturer,
    isLoading,
    isError,
  } = $api.useQuery("get", "/api/manufacturers/{id}", {
    params: { path: { id } },
  });

  const { data: files } = $api.useQuery(
    "get",
    "/api/manufacturers/{manufacturerId}/files" as never,
    { params: { path: { manufacturerId: id } } } as never,
  );

  const m = manufacturer as ManufacturerDetail | undefined;
  const fileList = (files as FileItem[] | undefined) ?? [];

  if (isLoading) {
    return (
      <main className="pl-page">
        <p className="text-muted-foreground">Loading…</p>
      </main>
    );
  }

  if (isError || !m) {
    return (
      <main className="pl-page">
        <Link to="/manufacturers" className="pl-back-link">
          <ArrowLeft size={16} /> Back to manufacturers
        </Link>
        <p className="text-destructive">Manufacturer not found.</p>
      </main>
    );
  }

  return (
    <main className="pl-page">
      <Link to="/manufacturers" className="pl-back-link">
        <ArrowLeft size={16} /> Back to manufacturers
      </Link>

      {/* Header */}
      <div className="flex items-center gap-5 mb-8">
        <EntityThumbnail
          src={m.hasThumbnail ? `/api/manufacturers/${m.id}/thumbnail` : null}
          alt={m.name}
          size="lg"
        />
        <div>
          <h1 className="text-3xl font-bold text-foreground mb-1">{m.name}</h1>
          {m.websiteUrl && (
            <a
              href={m.websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1 text-sm text-primary hover:underline"
            >
              <ExternalLink size={13} />
              {m.websiteUrl.replace(/^https?:\/\//, "")}
            </a>
          )}
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-8">
        <InfoCard label="Plates in catalog" value={String(m.plates.length)} />
      </div>

      {/* Plates */}
      {m.plates.length > 0 && (
        <section className="mb-8">
          <h2 className="pl-section-heading">Plates</h2>
          <div className="pl-table-container">
            <table className="pl-table">
              <thead className="pl-table-head">
                <tr>
                  <th className="pl-table-th">Name</th>
                  <th className="pl-table-th">Catalog No.</th>
                  <th className="pl-table-th">Wells</th>
                </tr>
              </thead>
              <tbody className="pl-table-body">
                {m.plates.map((p) => (
                  <tr key={p.id} className="pl-table-row-interactive">
                    <td className="pl-table-td">
                      <Link
                        to="/plates/$id"
                        params={{ id: p.id }}
                        className="text-primary hover:underline"
                      >
                        {p.name}
                      </Link>
                    </td>
                    <td className="pl-table-td font-mono text-sm">
                      {p.catalogNumber}
                    </td>
                    <td className="pl-table-td">{p.wellCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* Files */}
      {fileList.length > 0 && (
        <section>
          <h2 className="pl-section-heading">Files</h2>
          <ul className="space-y-2">
            {fileList.map((f) => (
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
