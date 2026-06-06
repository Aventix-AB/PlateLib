import { Link } from "@tanstack/react-router";
import { FlaskConical } from "lucide-react";

export default function Header() {
  return (
    <header className="px-6 py-4 flex items-center gap-3 bg-white border-b border-gray-200 shadow-sm">
      <FlaskConical size={22} className="text-blue-600" />
      <Link
        to="/"
        className="text-xl font-bold text-gray-900 hover:text-blue-600 transition-colors"
      >
        PlateLib
      </Link>
    </header>
  );
}
