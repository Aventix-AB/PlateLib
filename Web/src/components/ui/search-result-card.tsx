import { EntityThumbnail } from "@/components/ui/thumbnail";
import type { components } from "@/lib/api/schema.gen";
import { cn } from "@/lib/utils";

type SearchEntityType = components["schemas"]["SearchEntityType"];

interface SearchResultCardProps {
  thumbnailSrc: string | null;
  thumbnailAlt: string;
  badge: string;
  badgeVariant?: SearchEntityType;
  title: string;
  subtitle?: string;
  metadata?: { label: string; value: string }[];
  onClick?: () => void;
  className?: string;
}

/**
 * Base card used to display a single search result.
 * Plate and manufacturer results compose this component and supply their
 * own badge label and metadata items.
 */
export function SearchResultCard({
  thumbnailSrc,
  thumbnailAlt,
  badge,
  badgeVariant,
  title,
  subtitle,
  metadata = [],
  onClick,
  className,
}: SearchResultCardProps) {
  const badgeClasses =
    badgeVariant === "Manufacturer"
      ? "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-300"
      : "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300";

  return (
    <div
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onClick={onClick}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key === "Enter" || e.key === " ") onClick();
            }
          : undefined
      }
      className={cn(
        "flex items-center gap-4 rounded-xl border border-border bg-card px-4 py-3 shadow-sm transition-colors",
        onClick &&
          "cursor-pointer hover:border-primary/40 hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        className,
      )}
    >
      <EntityThumbnail src={thumbnailSrc} alt={thumbnailAlt} size="md" />

      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2 mb-0.5">
          <span
            className={cn(
              "inline-block rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide",
              badgeClasses,
            )}
          >
            {badge}
          </span>
          <span className="truncate font-semibold text-foreground">
            {title}
          </span>
        </div>

        {subtitle && (
          <p className="text-sm text-muted-foreground truncate">{subtitle}</p>
        )}

        {metadata.length > 0 && (
          <dl className="mt-1 flex flex-wrap gap-x-4 gap-y-0.5">
            {metadata.map(({ label, value }) => (
              <div
                key={label}
                className="flex items-center gap-1 text-xs text-muted-foreground"
              >
                <dt className="sr-only">{label}</dt>
                <dd>{value}</dd>
              </div>
            ))}
          </dl>
        )}
      </div>
    </div>
  );
}
