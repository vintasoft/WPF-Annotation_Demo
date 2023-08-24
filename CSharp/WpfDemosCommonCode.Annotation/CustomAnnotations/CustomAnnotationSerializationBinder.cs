using System;

using Vintasoft.Imaging.Annotation.Formatters;

namespace WpfDemosCommonCode.Annotation
{
    public class CustomAnnotationSerializationBinder : AnnotationSerializationBinder
    {

        #region Constructors

        public CustomAnnotationSerializationBinder()
            : base()
        {
        }

        #endregion


        #region Methdos

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.StartsWith("AnnotationDemo"))
                assemblyName = "Wpf" + assemblyName;

            if (typeName == "AnnotationDemo.TriangleAnnotation")
                typeName = "WpfAnnotationDemo.TriangleAnnotationData";
                       
            if (typeName == "AnnotationDemo.MarkAnnotationData")
                typeName = "WpfAnnotationDemo.MarkAnnotationData";
                       
            if (typeName.StartsWith("AnnotationDemo"))
                typeName = "Wpf" + typeName;

            if (typeName == "WpfAnnotationDemo.TriangleAnnotationData")
                typeName = "WpfDemosCommonCode.Annotation.TriangleAnnotationData";

            if (typeName == "WpfAnnotationDemo.MarkAnnotationData")
                typeName = "WpfDemosCommonCode.Annotation.MarkAnnotationData";

            if (typeName.StartsWith("DemosCommonCode"))
                typeName = "Wpf" + typeName;            

            return base.BindToType(assemblyName, typeName);
        }

        #endregion

    }
}
