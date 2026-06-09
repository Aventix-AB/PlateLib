import { useNavigate } from "@tanstack/react-router";
import { SearchResultCard } from "@/components/ui/search-result-card";
import type { components } from "@/lib/api/schema.gen";

type SearchResultItem = components["schemas"]["SearchResultItem"];

interface ManufacturerSearchResultProps {
  item: SearchResultItem;
}

export function ManufacturerSearchResult({
  item,
}: ManufacturerSearchResultProps) {
  const navigate = useNavigate();

  const metadata = item.websiteUrl
    ? [{ label: "Website", value: item.websiteUrl.replace(/^https?:\/\//, "") }]
    : [];

  return (
    <SearchResultCard
      thumbnailSrc={
        item.hasThumbnail ? `/api/manufacturers/${item.id}/thumbnail` : null
      }
      thumbnailAlt={item.name}
      badge="Manufacturer"
      badgeVariant="Manufacturer"
      title={item.name}
      subtitle={item.websiteUrl?.replace(/^https?:\/\//, "") ?? undefined}
      metadata={metadata}
      onClick={() =>
        void navigate({ to: "/manufacturers/$id", params: { id: item.id } })
      }
    />
  );
}
