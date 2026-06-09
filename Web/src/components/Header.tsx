import { Link } from "@tanstack/react-router";
import { FlaskConical } from "lucide-react";

export default function Header() {
  return (
    <header className="px-6 py-3 flex items-center justify-between bg-card border-b border-border shadow-sm">
      <div className="flex items-center gap-6">
        <Link to="/" className="flex items-center gap-2.5 group">
          <FlaskConical size={22} className="text-primary" />
          <span className="text-xl font-bold text-foreground group-hover:text-primary transition-colors">
            PlateLib
          </span>
        </Link>
        <nav className="flex items-center gap-4 text-sm">
          <Link
            to="/plates"
            className="text-muted-foreground hover:text-foreground transition-colors [&.active]:text-foreground [&.active]:font-medium"
          >
            Plates
          </Link>
          <Link
            to="/manufacturers"
            className="text-muted-foreground hover:text-foreground transition-colors [&.active]:text-foreground [&.active]:font-medium"
          >
            Manufacturers
          </Link>
        </nav>
      </div>
      <a
        href="https://aventix.io"
        target="_blank"
        rel="noopener noreferrer"
        className="text-xs text-muted-foreground hover:text-foreground transition-colors"
      >
        by Aventix
      </a>
    </header>
  );
}
