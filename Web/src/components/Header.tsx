import { Link } from "@tanstack/react-router";
import { FlaskConical } from "lucide-react";

export default function Header() {
  return (
    <header className="px-6 py-3 flex items-center justify-between bg-white border-b border-gray-200 shadow-sm">
      <Link to="/" className="flex items-center gap-2.5 group">
        <FlaskConical size={22} className="text-blue-600" />
        <span className="text-xl font-bold text-gray-900 group-hover:text-blue-600 transition-colors">
          PlateLib
        </span>
      </Link>
      <a
        href="https://aventix.io"
        target="_blank"
        rel="noopener noreferrer"
        className="text-xs text-gray-400 hover:text-gray-600 transition-colors"
      >
        by Aventix
      </a>
    </header>
  );
}
