import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { z } from "zod";
import { useState, useEffect } from "react";
import { Search } from "lucide-react";
import { $api } from "@/lib/api/client";
import type { components } from "@/lib/api/schema.gen";
import { PlateSearchResult } from "@/components/search/PlateSearchResult";
import { ManufacturerSearchResult } from "@/components/search/ManufacturerSearchResult";

type SearchEntityType = components["schemas"]["SearchEntityType"];

const ENTITY_TYPES = {
  Plate: "Plate" as SearchEntityType,
  Manufacturer: "Manufacturer" as SearchEntityType,
};

const searchSchema = z.object({
  q: z.string().optional().default(""),
});

export const Route = createFileRoute("/")({
  validateSearch: searchSchema,
  component: SearchPage,
});

function SearchPage() {
  const navigate = useNavigate({ from: "/" });
  const { q } = Route.useSearch();

  const [inputValue, setInputValue] = useState(q);

  // Sync input when URL param changes externally (e.g. back navigation)
  useEffect(() => {
    setInputValue(q);
  }, [q]);

  // Debounce input → URL param
  useEffect(() => {
    const timer = setTimeout(() => {
      if (inputValue !== q) {
        void navigate({ search: () => ({ q: inputValue }) });
      }
    }, 300);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [inputValue]);

  const { data, isLoading } = $api.useQuery(
    "get",
    "/api/search",
    { params: { query: { q: q || undefined } } },
    { enabled: !!q },
  );

  const items = data?.items ?? [];
  const hasQuery = !!q;

  return (
    <main className="flex flex-col items-center px-4 py-16 min-h-[70vh]">
      {/* Hero */}
      <div className="text-center mb-10 max-w-xl w-full">
        <h1 className="text-4xl font-bold tracking-tight text-foreground mb-3">
          Find any plate or manufacturer
        </h1>
        <p className="text-muted-foreground text-base">
          Search the PlateLib catalog — microwell plates, catalog numbers,
          manufacturers and more.
        </p>
      </div>

      {/* Search bar */}
      <div className="relative w-full max-w-xl mb-8">
        <Search
          size={18}
          className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none"
        />
        <input
          type="search"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="Search plates, catalog numbers, manufacturers…"
          aria-label="Search PlateLib"
          autoFocus
          className="pl-input w-full pl-10 py-3 text-base"
        />
      </div>

      {/* Results */}
      <div className="w-full max-w-xl">
        {isLoading && (
          <p className="text-center text-sm text-muted-foreground py-6">
            Searching…
          </p>
        )}

        {!isLoading && hasQuery && items.length === 0 && (
          <p className="text-center text-sm text-muted-foreground py-6">
            No results for <strong>"{q}"</strong>
          </p>
        )}

        {!isLoading && items.length > 0 && (
          <div className="flex flex-col gap-2">
            {items.map((item) =>
              item.entityType === ENTITY_TYPES.Plate ? (
                <PlateSearchResult key={`plate-${item.id}`} item={item} />
              ) : (
                <ManufacturerSearchResult
                  key={`manufacturer-${item.id}`}
                  item={item}
                />
              ),
            )}
            {(data?.totalCount ?? 0) > items.length && (
              <p className="text-center text-xs text-muted-foreground pt-2">
                Showing {items.length} of {data!.totalCount} results
              </p>
            )}
          </div>
        )}
      </div>
    </main>
  );
}

