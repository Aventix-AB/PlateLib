import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowLeft, FileDown } from "lucide-react";
import { $api } from "@/lib/api/client";

export const Route = createFileRoute("/plates/$id")({
  component: PlateDetailPage,
});

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
      <main className="max-w-4xl mx-auto px-4 py-8">
        <div className="text-gray-400">Loading…</div>
      </main>
    );
  }

  if (isError || !plate) {
    return (
      <main className="max-w-4xl mx-auto px-4 py-8">
        <Link
          to="/"
          className="inline-flex items-center gap-1 text-sm text-blue-600 hover:underline mb-6"
        >
          <ArrowLeft size={16} /> Back to plates
        </Link>
        <div className="text-red-500">Plate not found.</div>
      </main>
    );
  }

  return (
    <main className="max-w-4xl mx-auto px-4 py-8">
      <Link
        to="/"
        className="inline-flex items-center gap-1 text-sm text-blue-600 hover:underline mb-6"
      >
        <ArrowLeft size={16} /> Back to plates
      </Link>

      <h1 className="text-3xl font-bold text-gray-900 mb-1">{plate.name}</h1>
      <p className="text-gray-500 mb-8 font-mono text-sm">
        {plate.catalogNumber}
      </p>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 mb-8">
        <InfoCard label="Manufacturer" value={plate.manufacturerName} />
        <InfoCard label="Wells" value={String(plate.wellCount)} />
        <InfoCard
          label="Material"
          value={`${plate.material.code} — ${plate.material.name}`}
        />
      </div>

      {plate.properties.length > 0 && (
        <section className="mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-3">
            Properties
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
            {plate.properties.map((p) => (
              <div
                key={p.name}
                className="bg-gray-50 border border-gray-200 rounded-lg px-4 py-3"
              >
                <div className="text-xs text-gray-500 uppercase tracking-wide mb-0.5">
                  {p.name}
                </div>
                <div className="font-medium text-gray-800">{p.value}</div>
              </div>
            ))}
          </div>
        </section>
      )}

      {plate.files.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold text-gray-900 mb-3">Files</h2>
          <ul className="space-y-2">
            {plate.files.map((f) => (
              <li key={f.id}>
                <a
                  href={`/api/files/${f.id}/download`}
                  download={f.fileName}
                  className="inline-flex items-center gap-2 text-sm text-blue-600 hover:underline"
                >
                  <FileDown size={16} />
                  {f.fileName}
                  <span className="text-gray-400 text-xs">
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

function InfoCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-white border border-gray-200 rounded-lg px-4 py-3 shadow-sm">
      <div className="text-xs text-gray-500 uppercase tracking-wide mb-0.5">
        {label}
      </div>
      <div className="font-semibold text-gray-800">{value}</div>
    </div>
  );
}
