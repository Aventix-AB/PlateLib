import { cn } from "@/lib/utils";

interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

export function Card({ className, children, ...props }: CardProps) {
  return (
    <div className={cn("pl-card", className)} {...props}>
      {children}
    </div>
  );
}

export function CardBody({ className, children, ...props }: CardProps) {
  return (
    <div className={cn("pl-card-body", className)} {...props}>
      {children}
    </div>
  );
}

interface InfoCardProps {
  label: string;
  value: React.ReactNode;
  className?: string;
}

export function InfoCard({ label, value, className }: InfoCardProps) {
  return (
    <div className={cn("pl-card pl-card-body", className)}>
      <div className="pl-card-label">{label}</div>
      <div className="pl-card-value">{value}</div>
    </div>
  );
}
