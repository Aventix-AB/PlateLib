import { Building2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface EntityThumbnailProps {
  /** URL or API path to the thumbnail image. Pass null/undefined when not available. */
  src?: string | null;
  alt: string;
  size?: "sm" | "md" | "lg";
  className?: string;
}

const sizeMap = {
  sm: "w-8 h-8",
  md: "w-12 h-12",
  lg: "w-20 h-20",
};

const iconSizeMap = { sm: 16, md: 20, lg: 32 };

/**
 * Renders an entity thumbnail image, falling back to a muted placeholder
 * with a generic icon when no image is available.
 */
export function EntityThumbnail({
  src,
  alt,
  size = "md",
  className,
}: EntityThumbnailProps) {
  const sizeClass = sizeMap[size];

  if (!src) {
    return (
      <div
        className={cn("pl-thumbnail-fallback", sizeClass, className)}
        aria-label={alt}
      >
        <Building2 size={iconSizeMap[size]} />
      </div>
    );
  }

  return (
    <img
      src={src}
      alt={alt}
      className={cn("pl-thumbnail", sizeClass, className)}
      onError={(e) => {
        // Swap to fallback div on broken image
        const el = e.currentTarget;
        el.style.display = "none";
        const fallback = document.createElement("div");
        fallback.className = el.className.replace("pl-thumbnail", "pl-thumbnail-fallback");
        el.parentNode?.insertBefore(fallback, el);
      }}
    />
  );
}
