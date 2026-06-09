import { useNavigate } from "@tanstack/react-router";
import { SearchResultCard } from "@/components/ui/search-result-card";
import type { components } from "@/lib/api/schema.gen";

type SearchResultItem = components["schemas"]["SearchResultItem"];

interface PlateSearchResultProps {
  item: SearchResultItem;
}

export function PlateSearchResult({ item }: PlateSearchResultProps) {
  const navigate = useNavigate();

  const metadata = [
    item.catalogNumber
      ? { label: "Catalog No.", value: item.catalogNumber }
      : null,
    item.wellCount != null
      ? { label: "Wells", value: `${item.wellCount} wells` }
      : null,
    item.manufacturerName
      ? { label: "Manufacturer", value: item.manufacturerName }
      : null,
  ].filter(Boolean) as { label: string; value: string }[];

  return (
    <SearchResultCard
      thumbnailSrc={
        item.hasThumbnail ? `/api/plates/${item.id}/thumbnail` : null
      }
      thumbnailAlt={item.name}
      badge="Plate"
      badgeVariant="Plate"
      title={item.name}
      subtitle={item.catalogNumber ?? undefined}
      metadata={metadata}
      onClick={() =>
        void navigate({ to: "/plates/$id", params: { id: item.id } })
      }
    />
  );
}
