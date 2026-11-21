/**
 * @date 2026-11-21
 * @purpose Reusable IntersectionObserver hook triggering one-shot reveal animations with reduced-motion fallback.
 */
import { useEffect, useRef, useState, type RefObject } from "react";

export interface UseInViewOnceOptions {
  threshold?: number;
  rootMargin?: string;
}

export function useInViewOnce<T extends HTMLElement = HTMLDivElement>(
  options: UseInViewOnceOptions = {},
): [RefObject<T | null>, boolean] {
  const { threshold = 0.12, rootMargin = "0px 0px -40px 0px" } = options;
  const ref = useRef<T | null>(null);
  const [isVisible, setIsVisible] = useState<boolean>(() => {
    if (typeof window === "undefined") {
      return false;
    }
    const reducedMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;
    return Boolean(reducedMotion || typeof IntersectionObserver === "undefined");
  });

  useEffect(() => {
    if (isVisible) {
      return;
    }
    const element = ref.current;
    if (!element || typeof IntersectionObserver === "undefined") {
      setIsVisible(true);
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          setIsVisible(true);
          observer.disconnect();
        }
      },
      { threshold, rootMargin },
    );

    observer.observe(element);
    return () => observer.disconnect();
  }, [isVisible, threshold, rootMargin]);

  return [ref, isVisible];
}
