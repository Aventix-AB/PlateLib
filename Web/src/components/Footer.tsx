import { Github, Globe, Mail } from "lucide-react";

export default function Footer() {
  return (
    <footer className="mt-auto border-t border-gray-200 bg-white">
      <div className="max-w-7xl mx-auto px-6 py-6 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <span>
            <span className="font-medium text-gray-700">PlateLib</span> is open
            source, built and hosted by
          </span>
          <a
            href="https://aventix.io"
            target="_blank"
            rel="noopener noreferrer"
            aria-label="Aventix"
          >
            <img
              src="/Aventix_Logo.png"
              alt="Aventix"
              className="h-4 inline-block"
            />
          </a>
        </div>

        <nav
          aria-label="Footer links"
          className="flex items-center gap-5 text-gray-400"
        >
          <a
            href="https://github.com/Aventix-AB/platelib"
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-1.5 text-sm hover:text-gray-700 transition-colors"
            aria-label="PlateLib on GitHub"
          >
            <Github size={15} />
            <span>GitHub</span>
          </a>
          <a
            href="https://aventix.io"
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-1.5 text-sm hover:text-gray-700 transition-colors"
            aria-label="Aventix website"
          >
            <Globe size={15} />
            <span>aventix.io</span>
          </a>
          <a
            href="mailto:hello@aventix.io"
            className="flex items-center gap-1.5 text-sm hover:text-gray-700 transition-colors"
            aria-label="Contact Aventix"
          >
            <Mail size={15} />
            <span>hello@aventix.io</span>
          </a>
        </nav>
      </div>
    </footer>
  );
}
