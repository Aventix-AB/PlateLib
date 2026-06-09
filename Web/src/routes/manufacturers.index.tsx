import { createFileRoute, Link } from "@tanstack/react-router";
import { Globe } from "lucide-react";
import { $api } from "@/lib/api/client";
import { EntityThumbnail } from "@/components/ui/thumbnail";

// These types will be replaced by generated schema types after `pnpm generate:api`
interface ManufacturerListItem {
  id: string;
  name: string;
  websiteUrl?: string | null;
  hasThumbnail: boolean;
}

export const Route = createFileRoute("/manufacturers/")({
  component: ManufacturersPage,
});

function ManufacturerCard({ m }: { m: ManufacturerListItem }) {
  return (
    <Link
      to="/manufacturers/$id"
      params={{ id: m.id }}
      className="pl-entity-card group no-underline"
    >
      <EntityThumbnail
        src={m.hasThumbnail ? `/api/manufacturers/${m.id}/thumbnail` : null}
        alt={m.name}
        size="lg"
      />
      <div className="text-center">
        <div className="font-semibold text-foreground group-hover:text-primary transition-colors">
          {m.name}
        </div>
        {m.websiteUrl && (
          <div className="mt-1 flex items-center justify-center gap-1 text-xs text-muted-foreground">
            <Globe size={11} />
            <span className="truncate max-w-[140px]">
              {m.websiteUrl.replace(/^https?:\/\//, "")}
            </span>
          </div>
        )}
      </div>
    </Link>
  );
}

function ManufacturersPage() {
  const { data, isLoading } = $api.useQuery("get", "/api/manufacturers");

  const manufacturers = (data as ManufacturerListItem[] | undefined) ?? [];

  return (
    <main className="pl-page">
      <div className="pl-page-header">
        <h1 className="pl-page-title">Manufacturers</h1>
        <p className="pl-page-subtitle">
          Browse all plate manufacturers in the catalog
        </p>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground text-sm">Loading…</p>
      ) : manufacturers.length === 0 ? (
        <p className="text-muted-foreground text-sm">No manufacturers found.</p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {manufacturers.map((m) => (
            <ManufacturerCard key={m.id} m={m} />
          ))}
        </div>
      )}
    </main>
  );
}
