import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import { itemVariants } from "@/lib/utils";
import { motion } from 'framer-motion';
import type { LucideIcon } from "lucide-react";
 
interface StatCardProps {
    title: string,
    value: string,
    icon: LucideIcon,
    color: string,
    description: string
}

export const StatCard = ({ title, value, icon: Icon, color, description }: StatCardProps) => (
    <motion.div variants={itemVariants}>
      <Card className="bg-card/40 border-border/50 overflow-hidden relative backdrop-blur-sm hover:bg-card/60 transition-colors">
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle className="text-sm font-medium">{title}</CardTitle>
          <Icon className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold">{value}</div>
          <p className="text-xs text-muted-foreground mt-1">
            {description}
          </p>
          <div 
            className="absolute bottom-0 right-0 w-24 h-24 -mr-8 -mb-8 opacity-[0.05]" 
            style={{ color }}
          >
            <Icon size={96} />
          </div>
        </CardContent>
      </Card>
    </motion.div>
  );

  