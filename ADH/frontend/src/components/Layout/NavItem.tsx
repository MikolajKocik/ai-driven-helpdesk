import { cn } from "@/lib/utils";
import { Button } from "../ui/Button";
import { type LucideIcon } from "lucide-react";

interface NavItemProps {
    icon: LucideIcon,
    label: string,
    isActive: boolean,
    onClick: () => void
}  

export const NavItem = ({ icon: Icon, label, isActive, onClick }: NavItemProps) => (
    <Button
        variant={isActive ? "secondary" : "ghost"}
        className={cn(
            "w-full justify-start gap-3 text-base h-11",
            isActive && "bg-secondary font-semibold"
        )}
        onClick={onClick}
    >
        <Icon size={20} className={isActive ? "text-primary" : "text-muted-foreground"} />
        {label}
    </Button>
  );